# PowerConfig
An elegant dynamic XML configuration library supporting three styles: static methods, indexers, and dynamic properties, with automatic persistence and infinite nesting.


# PowerConfig 使用说明书

欢迎使用 **PowerConfig** —— 一个为 .NET 应用程序设计的轻量级、零配置的动态 XML 配置库。它提供三种风格 API（静态方法、索引器、动态属性），支持无限层级嵌套，所有修改自动持久化到 `pwrcfg.xml` 文件中。
最新稳定版本1.0.4.可在nuget中直接安装后使用。
---

## ✨ 主要特性

- **零配置启动**：首次使用自动生成配置文件，无需初始化。
- **三种风格 API**：
  - **静态方法**：`Config.Set` / `Config.Get`，适合键名动态生成的场景。
  - **索引器**：`Config.Root["key"]`，通过字符串键名访问，灵活且自然。
  - **动态属性**：`Config.Root.Level1.Level2 = value`，像操作变量一样读写任意层级。
- **无限层级嵌套**：任意层级的动态赋值都会自动创建中间节点，无需预定义结构。
- **短路缺失节点**：当访问的节点路径中任何一级不存在时，立即返回 `MissingNode`，后续所有访问均不报错。
- **智能隐式转换**：将动态属性赋值给 `string` 变量时自动转换：
  - 叶子节点 → 存储的字符串值
  - 容器节点 → 子节点数量统计，如 `"2 children"`
  - 缺失节点 → **`"node or value not exist"`**
- **链式节点访问**：通过 `Config.Key` 获取节点对象，支持遍历子节点、判断存在性等。
- **自动持久化**：每次修改立即保存到 XML 文件，无需手动调用 `Save`。
- **线程安全**：内部锁保证多线程环境下的读写安全。
- **轻量级**：仅依赖 .NET 内置库，无外部依赖。

---

## 📦 安装

### 通过 NuGet 安装（推荐）
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
Config.Root.Person.Address.City = "New York";  // 自动创建 Address 节点
```

### 3. 读取配置

```csharp
// 叶子节点直接返回字符串
string appName = Config.Get("AppName");                 // "MyApp"
string server = Config.Root.Database.Server;            // "localhost"

// 容器节点返回统计信息
string yoloInfo = Config.Root.Database;                 // "2 children"

// 不存在的节点返回友好提示
string nonExist = Config.Root.Some.Nonexistent.Path;    // "node or value not exist"
string deepNonExist = Config.Root.a.b.c.d.e.f;          // "node or value not exist"

// 索引器方式（返回 null 或值）
string port = Config.Root["Database.Port"];              // "1433"
string missing = Config.Root["Some.Missing"];            // null
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
| `bool ContainsKey(string key)` | 判断指定路径的节点是否存在（作为叶子或容器）。 |
| `ConfigNodeBase Key(string path)` | 获取节点对象，用于链式遍历。 |

### 节点基类 `ConfigNodeBase`

所有动态属性返回的对象都派生自此类，包含以下成员：

| 成员 | 描述 |
|------|------|
| `ConfigNodeBase Key(string name)` | 获取子节点代理。 |
| `IEnumerable<string> GetKeys()` | 获取当前节点下的直接子键名。 |
| `void SetValue(string value)` | 设置当前节点的值（会使其成为叶子）。 |
| `bool Exists()` | 判断当前节点是否存在。 |
| `override string ToString()` | 返回字符串表示（与隐式转换相同）。 |
| `static implicit operator string(ConfigNodeBase node)` | 隐式转换为字符串。 |

### 隐式转换规则

将节点对象赋值给 `string` 变量时自动触发：

| 节点类型 | 转换结果 |
|----------|----------|
| 叶子节点 | 存储的字符串值 |
| 容器节点 | `"{n} children"`（n 为直接子节点数量） |
| 缺失节点 | `"node or value not exist"` |

---

## 📘 常见使用案例

### 1. 动态属性赋值与读取
```csharp
Config.Root.User.Name = "Alice";
Config.Root.User.Email = "alice@example.com";
string name = Config.Root.User.Name;          // "Alice"
string email = Config.Root.User.Email;         // "alice@example.com"
string userInfo = Config.Root.User;            // "2 children"
```

### 2. 遍历 Database 下的所有子属性
```csharp
var dbNode = Config.Key("Database");
if (dbNode.Exists())
{
    Console.WriteLine($"Database has {dbNode}"); // 隐式转换，如 "Database has 2 children"
    foreach (var key in dbNode.GetKeys())
    {
        var child = dbNode.Key(key);
        if (child.GetKeys().Any())
            Console.WriteLine($"  {key}/ ({child})");
        else
            Console.WriteLine($"  {key} = {child}");
    }
}
```

### 3. 递归遍历所有配置
```csharp
void Traverse(ConfigNodeBase node, string indent = "")
{
    foreach (var key in node.GetKeys())
    {
        var child = node.Key(key);
        if (child.GetKeys().Any())
        {
            Console.WriteLine($"{indent}{key}/ ({child})");
            Traverse(child, indent + "  ");
        }
        else
        {
            Console.WriteLine($"{indent}{key} = {child}");
        }
    }
}
Traverse(Config.Key(""));
```

### 4. 使用默认值
```csharp
int port = int.Parse(Config.Get("Database.Port") ?? "1433");
// 或通过动态属性 + 隐式转换
string portStr = Config.Root.Database.Port; // 如果不存在，返回 "node or value not exist"
if (portStr != "node or value not exist" && int.TryParse(portStr, out int p))
    port = p;
else
    port = 1433;
```

### 5. 修改深层节点的值
```csharp
Config.Key("Database").Key("Port").SetValue("5432");
// 或直接动态属性赋值
Config.Root.Database.Port = "5432";
```

### 6. 检查节点是否存在
```csharp
if (Config.Key("Database.Credentials").Exists())
{
    // 存在
}
```

### 7. 给缺失节点赋值（自动创建）
```csharp
Config.Root.NewSection.NewSubSection.Value = "created";
string val = Config.Root.NewSection.NewSubSection.Value; // "created"
```

### 8. 安全读取不存在节点（无需 try-catch）
```csharp
string result = Config.Root.A.B.C.D.E.F; // 永远返回 "node or value not exist"
```

---

## ⚠️ 注意事项

- **所有值均为字符串**：存储时自动转为字符串，读取后需自行转换类型。
- **动态属性赋值规则**：
  - 给叶子节点赋字符串值：正常更新。
  - 给容器节点赋字符串值：节点变为叶子，原有子节点全部丢失。
  - 给缺失节点赋字符串值：自动创建所有中间节点，并设置叶子值。
- **隐式转换**：将动态属性赋值给 `string` 变量时自动触发，无需显式调用方法。
- **线程安全**：所有公共方法都是线程安全的。
- **配置文件位置**：默认保存在应用程序工作目录（`Environment.CurrentDirectory`），文件名为 `pwrcfg.xml`。
- **键名区分大小写**：`"Database.Server"` 与 `"database.server"` 不同。

---


## 🤝 贡献与许可

PowerConfig 采用 MIT 许可证，欢迎提交 Issue 或 Pull Request。

- GitHub 仓库：[https://github.com/haigear/PowerConfig](https://github.com/haigear/PowerConfig)
- 问题反馈：[https://github.com/haigear/PowerConfig/issues](https://github.com/haigear/PowerConfig/issues)

感谢您使用 PowerConfig！
