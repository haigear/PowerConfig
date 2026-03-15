# PowerConfig
An elegant dynamic XML configuration library supporting three styles: static methods, indexers, and dynamic properties, with automatic persistence and infinite nesting.


# PowerConfig 使用指南

欢迎使用 **PowerConfig** —— 一个为 .NET 应用程序设计的轻量级、零配置的动态 XML 配置库。它提供三种风格 API（静态方法、索引器、动态属性），支持无限层级嵌套，所有修改自动持久化到 `pwrcfg.xml` 文件中。

---

## ✨ 主要特性

- **零配置启动**：首次使用自动生成配置文件，无需初始化。
- **三种风格 API**：
  - 静态方法：`Config.Set` / `Config.Get`
  - 索引器：`Config.Root["key"]`
  - 动态属性：`Config.Root.Level1.Level2 = value`
- **链式节点访问**：通过 `Config.Key` 获取节点对象，支持遍历子节点、读取/设置值。
- **智能动态读取**：
  - 叶子节点（有值且无子元素）直接返回字符串。
  - 容器节点（有子元素）返回节点代理，可通过 `.GetValue()` 获取统计信息（如 `"3 children"`）。
- **自动创建中间节点**：任意层级的动态赋值都会自动创建缺失的节点。
- **线程安全**：内部锁保证多线程环境安全。
- **轻量级**：仅依赖 .NET 内置库。

---

## 📦 安装

### 通过 NuGet 安装（推荐）
在 Visual Studio 中打开“管理 NuGet 程序包”，搜索 `PowerConfig` 并安装；或使用命令行：
```bash
dotnet add package PowerConfig
```

### 或直接引用 DLL
将编译后的 `PowerConfig.dll` 添加到项目引用，并在代码文件顶部添加：
```csharp
using PowerConfig;
```

---

## 🚀 快速开始

### 1. 引入命名空间
```csharp
using PowerConfig;
```

### 2. 写入配置（三种风格）

```csharp
// 风格一：静态方法
Config.Set("AppName", "MyApp");
Config.Set("Database.Server", "localhost");

// 风格二：索引器（通过 Root）
Config.Root["Version"] = "1.0.0";
Config.Root["Database.Port"] = "1433";

// 风格三：动态属性（自动创建中间节点）
Config.Root.Database.Credentials.User = "sa";
Config.Root.Database.Credentials.Password = "123456";
Config.Root.Person.Name = "John";
Config.Root.Person.Age = "30";
Config.Root.Person.Address.City = "New York";  // 自动创建 Address 节点
```

### 3. 读取配置

```csharp
// 静态方法
string appName = Config.Get("AppName");                 // "MyApp"
string server = Config.Get("Database.Server");          // "localhost"

// 索引器
string version = Config.Root["Version"];                 // "1.0.0"

// 动态属性（叶子节点直接返回字符串）
string user = Config.Root.Database.Credentials.User;    // "sa"
string city = Config.Root.Person.Address.City;          // "New York"

// 动态属性（容器节点返回节点代理）
dynamic personNode = Config.Root.Person;                 // 返回 ConfigNode 代理
string personInfo = personNode.GetValue();                // 返回 "3 children" (Name, Age, Address)

// 通过链式节点访问获取统计信息
var dbNode = Config.Key("Database");
string dbInfo = dbNode.GetValue();                        // 返回 "2 children" (Server, Credentials)
```

### 4. 生成的 XML 文件（`pwrcfg.xml`）

```xml
<Configuration>
  <AppName>MyApp</AppName>
  <Version>1.0.0</Version>
  <Database>
    <Server>localhost</Server>
    <Port>1433</Port>
    <Credentials>
      <User>sa</User>
      <Password>123456</Password>
    </Credentials>
  </Database>
  <Person>
    <Name>John</Name>
    <Age>30</Age>
    <Address>
      <City>New York</City>
    </Address>
  </Person>
</Configuration>
```

---

## 🧰 API 详解

### 静态类 `Config`

| 成员 | 描述 |
|------|------|
| `dynamic Root` | 动态根对象，用于动态属性赋值和索引器访问。 |
| `dynamic Dynamic` | 兼容旧版的别名，指向同一实例。 |
| `void Set(string key, string value)` | 通过点号路径设置值（如 `"Database.Server"`）。 |
| `string Get(string key)` | 通过点号路径获取值，不存在返回 `null`。 |
| `IEnumerable<string> GetKeys()` | 获取所有顶级键名。 |
| `bool ContainsKey(string key)` | 判断指定路径的节点是否存在。 |
| `ConfigNode Key(string path)` | 获取节点对象，用于链式遍历。 |

### 类 `ConfigNode`（节点代理）

| 成员 | 描述 |
|------|------|
| `ConfigNode Key(string name)` | 获取子节点代理。 |
| `IEnumerable<string> GetKeys()` | 获取当前节点下的直接子键名。 |
| `string GetValue()` | 获取当前节点的值：<br>- 叶子节点返回存储的字符串。<br>- 容器节点返回统计信息，如 `"3 children"`。<br>- 不存在的节点返回 `null`。 |
| `void SetValue(string value)` | 设置当前节点的值（会覆盖原有内容）。 |
| `bool Exists()` | 判断当前节点是否存在。 |

---

## 📘 常见使用案例

### 1. 动态属性赋值与读取
```csharp
Config.Root.User.Name = "Alice";
Config.Root.User.Email = "alice@example.com";
string name = Config.Root.User.Name;          // "Alice"
string email = Config.Root.User.Email;         // "alice@example.com"
```

### 2. 遍历 Database 下的所有子属性
```csharp
var dbNode = Config.Key("Database");
Console.WriteLine($"Database has {dbNode.GetValue()}"); // 如 "Database has 2 children"

foreach (var key in dbNode.GetKeys())
{
    var child = dbNode.Key(key);
    if (child.GetKeys().Any()) // 有子节点
    {
        Console.WriteLine($"  {key}/ ({child.GetValue()})");
        foreach (var subKey in child.GetKeys())
        {
            string value = child.Key(subKey).GetValue();
            Console.WriteLine($"    {subKey} = {value}");
        }
    }
    else
    {
        string value = child.GetValue();
        Console.WriteLine($"  {key} = {value}");
    }
}
```

### 3. 递归遍历所有配置
```csharp
void Traverse(ConfigNode node, string indent = "")
{
    foreach (var key in node.GetKeys())
    {
        var child = node.Key(key);
        string val = child.GetValue();
        if (child.GetKeys().Any())
        {
            Console.WriteLine($"{indent}{key}/ ({val})");
            Traverse(child, indent + "  ");
        }
        else
        {
            Console.WriteLine($"{indent}{key} = {val}");
        }
    }
}
Traverse(Config.Key(""));
```

### 4. 使用默认值
```csharp
int port = int.Parse(Config.Get("Database.Port") ?? "1433");
```

### 5. 修改深层节点的值
```csharp
Config.Key("Database").Key("Port").SetValue("5432");
```

### 6. 检查节点是否存在
```csharp
if (Config.Key("Database.Credentials").Exists())
{
    // 存在
}
```

### 7. 叶子节点自动转换为容器
```csharp
Config.Root.Temp = "temp value";       // Temp 是叶子
Config.Root.Temp.Key = "new value";     // 自动将 Temp 转换为容器，原值丢失
string tempVal = Config.Root.Temp;       // 返回代理，不是字符串
string tempStr = Config.Root.Temp.GetValue(); // 返回 "1 child"
string keyVal = Config.Root.Temp.Key;    // "new value"
```

### 8. 直接使用 GetValue 获取统计信息
```csharp
var personNode = Config.Key("Person");
Console.WriteLine(personNode.GetValue()); // 输出 "3 children"
```

---

## ⚠️ 注意事项

- **所有值均为字符串**：读取后需自行转换类型（如 `int.Parse`）。
- **动态属性读取规则**：
  - 叶子节点（有值且无子元素）直接返回 `string`。
  - 容器节点（有子元素）返回 `ConfigNode` 代理，可通过 `.GetValue()` 获取统计信息。
  - 不存在的节点返回 `ConfigNode` 代理（允许后续赋值）。
- **动态属性赋值规则**：
  - 如果目标节点是叶子，赋值给它的子属性会自动将其转换为容器（原值丢失）。
  - 直接给节点赋字符串值（如 `Config.Root.Node = "value"`）会使其成为叶子。
- **配置文件位置**：默认保存在应用程序工作目录（`Environment.CurrentDirectory`），可修改 `DynamicConfig` 构造函数指定路径。
- **线程安全**：所有公共方法都是线程安全的，可放心在多线程环境使用。
- **键名区分大小写**：`"Database.Server"` 与 `"database.server"` 不同。

---

## 🤝 贡献与许可

PowerConfig 采用 MIT 许可证，欢迎提交 Issue 或 Pull Request。

- GitHub 仓库：[https://github.com/您的用户名/PowerConfig](https://github.com/您的用户名/PowerConfig)
- 问题反馈：[https://github.com/您的用户名/PowerConfig/issues](https://github.com/您的用户名/PowerConfig/issues)

感谢您使用 PowerConfig！
