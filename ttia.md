# AudioShare 使用与打包指南

## 功能概览

AudioShare 是一款实时音频流传输工具，支持将 Windows PC 的音频实时传输到 Android 设备播放。主要特性包括：

- **多声道支持**：Mono、Stereo、2.1、4.0、5.1、7.1 等多种声道布局
- **智能混音**：自动声道上混/下混，适配不同设备
- **多音频 API**：AudioTrack、OpenSL ES、AAudio 三种输出方式
- **双连接模式**：支持 USB (ADB) 和 WiFi 连接
- **低延迟**：OpenSL ES 提供低延迟音频播放
- **远程控制**：内置 Musiche 音乐播放器支持远程控制
- **高安全性**：内置缓冲区大小限制，防止内存溢出攻击

## Windows 端使用说明（采集端）

### 1. 基本设置

1. 启动 `AudioShare.exe`
2. 在"音频播放设备"下拉框中选择需要镜像的输出设备
3. 设置采样率（推荐 44100Hz 或 48000Hz）

### 2. 声道配置

在设备列表的"声道"列中可以选择以下声道布局：

| 声道模式 | 声道数 | 适用场景 |
|---------|--------|----------|
| **Stereo（立体声）** | 2 | 标准双声道设备（默认推荐） |
| **Mono（单声道）** | 1 | 单个扬声器或混音输出 |
| **Left/Right（左/右声道）** | 1 | 仅输出单个声道（兼容旧版） |
| **2.1 Surround** | 3 | 立体声 + 低音炮 |
| **4.0 Surround** | 4 | 前置 L/R + 后置 L/R |
| **5.1 Surround** | 6 | 家庭影院标准配置 |
| **7.1 Surround** | 8 | 高级环绕声系统 |

### 3. 智能混音说明

AudioShare 支持自动声道适配：

- **上混（Upmix）**：当 PC 输出声道少于目标布局时
  - 缺失的声道会由可用声道复制并适当衰减
  - 确保所有扬声器都有声音输出

- **下混（Downmix）**：当 PC 输出声道多于目标布局时
  - 按行业标准将中置、环绕声平滑混入前置声道
  - 保持音频平衡，避免信息丢失

**示例**：
- 5.1 音频 → Stereo 输出：中置和环绕声会智能混入左右声道
- Stereo 音频 → 5.1 输出：左右声道会复制到相应位置，保持声场平衡

### 4. 设备连接

#### USB 连接（ADB）
1. 使用 USB 线连接 Android 设备到 PC
2. 在 Android 设备上启用"USB 调试"（开发者选项）
3. Windows 端会自动检测并列出 USB 设备
4. 配置声道后点击"连接"

#### WiFi 连接
1. 确保 PC 和 Android 设备在同一局域网
2. 在 Windows 端点击"添加设备"
3. 输入 Android 设备显示的 IP 地址和端口（格式：`192.168.1.100:8088`）
4. 配置声道后点击"连接"

### 5. 日志查看

点击"日志"窗口可以查看：
- 音频捕获状态（`set audio data start/end`）
- 连接状态
- 错误信息

## Android 端使用说明（播放端）

### 1. 音频输出 API 选择

打开 App 后，顶部有"输出 API"选择器，提供三种模式：

| API 模式 | 延迟 | 兼容性 | 说明 |
|---------|------|--------|------|
| **AudioTrack** | 正常 | Android 全版本 | 标准兼容模式，适合旧设备 |
| **OpenSL ES** | 低 | Android 2.3+ | **原生低延迟实现（默认推荐）** |
| **AAudio（低延迟）** | 最低 | Android 8.0+ | 预留模式，当前使用低延迟 AudioTrack |

**推荐设置**：
- 实时音频场景（音乐、游戏）：选择 **OpenSL ES**
- 旧设备或遇到问题时：选择 **AudioTrack**
- Android 8.0+ 新设备：可尝试 **AAudio**

### 2. 使用步骤

1. 启动 AudioShare App
2. 选择音频输出 API（首次使用推荐 OpenSL ES）
3. 记录显示的 IP 地址和端口号
4. 在 Windows 端连接该地址
5. 设置会自动保存，下次无需重新选择

**注意事项**：
- API 切换会在下一次播放时生效，无需重启服务
- 如果 OpenSL ES 初始化失败，会自动降级到 AudioTrack
- Android 8.0 以下设备选择 AAudio 会自动使用 OpenSL ES

### 3. 连接状态

- **NotConnected**：未连接，等待 Windows 端连接
- **Connected**：已连接，正在播放音频

### 4. Musiche 音乐播放器

App 内置 HTTP 服务器，支持：
- 浏览器访问管理界面
- 远程播放本地音乐
- 控制播放/暂停/切歌
- 通过开关可以启用/禁用（重启生效）

## 打包流程

### Windows 桌面端打包

#### 环境要求
- .NET 6 SDK 或更高版本（推荐 .NET 8）
- Windows 10/11 操作系统

#### 打包命令

**推荐命令（指定目标框架）：**
```powershell
cd windows/
dotnet publish AudioShare.csproj -c Release -f "net6.0-windows10.0.17763.0" -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

**或使用简化命令：**
```powershell
cd windows/
dotnet publish AudioShare.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

#### 输出位置

```
windows/bin/Release/net6.0-windows10.0.17763.0/win-x64/publish/AudioShare.exe
```

#### 编译警告说明

**警告：NETSDK1138（目标框架过时）**
- 原因：.NET 6 已接近生命周期结束
- 影响：不影响使用，但不会收到安全更新
- 解决方案：
  - 方案1：忽略警告，继续使用（推荐，当前可用）
  - 方案2：升级到 .NET 8（需修改 `.csproj` 文件中的 `<TargetFramework>` 标签）

**注意**：所有编译警告均已修复，当前版本可正常编译和运行。

### Android 客户端打包

#### 环境要求
- **JDK 17 或 21**（⚠️ 不支持 Java 25）
  - 推荐：[Eclipse Temurin JDK 17 LTS](https://adoptium.net/temurin/releases/?version=17)
  - 原因：Gradle构建脚本编译器不支持Java 25字节码
- Android SDK（编译目标：API 34）
- NDK（会由 Gradle 自动下载）
- CMake 3.22.1+（用于编译 OpenSL ES 原生库）
- **Gradle 8.12**（已配置，支持 Java 17-21）

#### ⚠️ 重要提示
- **Java 25不兼容**：即使Gradle 8.12支持Java 25运行时，但其Groovy编译器无法编译`.gradle`脚本
- **推荐配置**：使用JDK 17 LTS或JDK 21 LTS
- **多Java版本共存**：可通过`JAVA_HOME`环境变量切换版本
- 首次编译需要下载 Gradle 和依赖，可能需要较长时间
- 如遇到版本冲突，请先清理缓存：`.\gradlew.bat clean --no-daemon`

#### 打包命令

**Debug 版本：**
```powershell
cd android/
.\gradlew.bat clean assembleDebug
```

**Release 版本：**
```powershell
cd android/
.\gradlew.bat clean assembleRelease
```

#### 输出位置

- **Debug APK**：`android/app/build/outputs/apk/debug/AudioShare-debug.apk`
- **Release APK**：`android/app/build/outputs/apk/release/AudioShare-release.apk`
- **Native 库**：`lib/armeabi-v7a/libaudioshare-opensl.so`（默认仅构建 armeabi-v7a）

#### 当前状态
> ⚠️ Gradle 下载超时导致打包失败。
>
> **解决方法**：
> 1. 手动删除锁文件：`C:\Users\CNQ\.gradle\wrapper\dists\gradle-8.5-bin\*\gradle-8.5-bin.zip.lck`
> 2. 确保网络可以访问 `gradle.org` 和 `services.gradle.org`
> 3. 可选：配置 Gradle 镜像源加速下载

#### Native 库编译

OpenSL ES 库通过 CMake 自动编译：

```cmake
# android/app/src/main/cpp/CMakeLists.txt
add_library(audioshare-opensl SHARED opensl_player.cpp)
target_link_libraries(audioshare-opensl log OpenSLES)
```

支持的 ABI：
- `armeabi-v7a`（默认启用）

> 如需 arm64-v8a / x86 / x86_64，请在 `android/app/build.gradle` 的 `defaultConfig.ndk.abiFilters` 中添加对应架构，或移除该配置后交由 Gradle 自动构建全部 ABI。

## 测试建议

### 1. Windows 端测试

在"日志"窗口观察：
```
[INFO] set audio data start
[INFO] set audio data end
[INFO] connect start
[INFO] connect send head
[INFO] connect end
```

### 2. Android 端测试

使用 ADB Logcat 查看日志：

```bash
adb logcat -s AudioShareService AudioShareOpenSL
```

关键日志：
- `play audio ready to read` - 开始接收音频数据
- `OpenSL` 初始化日志 - 验证 OpenSL ES 是否成功加载
- `write audio data err` - 写入错误（如果出现）

### 3. 多声道测试

**测试方法**：
1. 在 Windows 端播放 5.1 或 7.1 测试音频
2. 分别测试不同声道配置：
   - Stereo：验证下混是否正确
   - 5.1：验证各声道是否对应
   - Mono：验证单声道混音
3. 观察声场平衡和低音炮输出

**推荐测试音频**：
- Dolby 5.1/7.1 测试音频
- THX Deep Note
- 左右声道平衡测试

### 4. 音频 API 对比测试

在 Android 端切换不同 API，测试：
- 延迟差异（播放音乐或视频）
- CPU 占用（使用系统监控工具）
- 稳定性（长时间播放）
- 兼容性（不同设备）

## 发布打包

### 完整发布包结构

```
AudioShare-v2.0/
├── AudioShare.exe          # Windows 端主程序
├── AudioShare.apk          # Android 端安装包
├── adb.exe                 # ADB 工具（可选）
├── AdbWinApi.dll          # ADB 依赖（可选）
├── AdbWinUsbApi.dll       # ADB 依赖（可选）
├── README.md              # 使用说明
└── LICENSE                # 许可证
```

### 发布清单

- [ ] Windows exe 签名（可选）
- [ ] Android APK 签名（Release 必须）
- [ ] 版本号一致性检查
- [ ] 更新日志（updata.md）
- [ ] 使用文档（ttia.md）
- [ ] README 更新多声道和 API 说明
- [ ] 测试报告

## 故障排查

### 编译问题

#### Windows 端编译问题

**问题：NETSDK1138 警告（目标框架过时）**
- 状态：正常，不影响使用
- 原因：.NET 6 接近生命周期结束
- 解决：可忽略，或升级到 .NET 8

**问题：CS0168 警告（变量未使用）**
- 状态：已修复（v2.0.1）
- 原因：异常处理中声明但未使用的变量
- 解决：已在最新版本中移除

**问题：找不到 dotnet 命令**
- 原因：未安装 .NET SDK
- 解决：从 https://dotnet.microsoft.com/download 下载安装 .NET 6 或更高版本

**问题：编译成功但找不到输出文件**
- 检查路径：`windows/bin/Release/net6.0-windows10.0.17763.0/win-x64/publish/`
- 确认命令参数中包含 `-r win-x64`

#### Android 端编译问题

**问题：Unsupported class file major version 69（严重 - Java 25不兼容）**
- **错误信息**：`BUG! exception in phase 'semantic analysis' in source unit '_BuildScript_' Unsupported class file major version 69`
- **根本原因**：Gradle 8.12的Groovy编译器不支持Java 25字节码编译`.gradle`构建脚本
- **版本对应关系**：
  - Major version 69 = Java 25（当前系统版本）
  - Major version 65 = Java 21（推荐）
  - Major version 61 = Java 17（推荐）

**解决方案（3选1）：**

**方案1：临时设置JAVA_HOME到Java 17/21（最快）**
```powershell
# 如果你安装了多个Java版本，临时指定使用Java 17或21
$env:JAVA_HOME="C:\Program Files\Java\jdk-17"  # 替换为你的JDK 17路径
$env:PATH="$env:JAVA_HOME\bin;$env:PATH"

# 验证
java -version  # 应该显示Java 17或21

# 然后编译
cd android/
.\gradlew.bat clean assembleRelease
```

**方案2：下载并安装Java 17/21（推荐长期使用）**
1. 下载 JDK 17 LTS：https://adoptium.net/temurin/releases/?version=17
2. 安装后设置环境变量：
   ```powershell
   # 永久设置（管理员PowerShell）
   [System.Environment]::SetEnvironmentVariable('JAVA_HOME', 'C:\Program Files\Eclipse Adoptium\jdk-17.0.x-hotspot', 'Machine')
   ```
3. 重启PowerShell，运行 `java -version` 确认
4. 编译项目

**方案3：使用Gradle Daemon配置（无需更改系统Java）**
在 `android/gradle.properties` 添加：
```properties
# 指定Gradle使用的Java版本（如果安装了JDK 17）
org.gradle.java.home=C:/Program Files/Eclipse Adoptium/jdk-17.0.x-hotspot
```

**快速诊断命令：**
```powershell
# 检查当前Java版本
java -version

# 检查所有安装的Java
where java
dir "C:\Program Files\Java"
dir "C:\Program Files\Eclipse Adoptium"

# 清理Gradle缓存后重试
cd android/
.\gradlew.bat clean --no-daemon
```

**问题：SDK location not found（常见）**
- **错误信息**：`SDK location not found. Define a valid SDK location with an ANDROID_HOME environment variable or by setting the sdk.dir path in your project's local properties file`
- **原因**：未配置Android SDK路径
- **解决方案（已自动配置）**：
  - 已创建 `android/local.properties` 文件
  - 默认SDK路径：`C:\Users\<用户名>\AppData\Local\Android\Sdk`

- **手动配置方法**：
  ```powershell
  # 方法1：创建 local.properties 文件
  cd android/
  echo "sdk.dir=C:\\Users\\CNQ\\AppData\\Local\\Android\\Sdk" > local.properties

  # 方法2：设置环境变量（永久）
  [System.Environment]::SetEnvironmentVariable('ANDROID_HOME', 'C:\Users\CNQ\AppData\Local\Android\Sdk', 'User')
  ```

- **查找SDK路径**：
  ```powershell
  # 常见位置
  dir "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
  dir "C:\Android\Sdk"

  # 通过Android Studio查找
  # File → Settings → Appearance & Behavior → System Settings → Android SDK
  ```

**问题：JDK 版本不兼容（旧问题描述）**
- **要求**：JDK 17-25（推荐 17 或 21）
- **检查**：`java -version`
- **解决**：确保使用兼容的 JDK 版本

**问题：Gradle 下载超时**
- 原因：网络无法访问 gradle.org
- 解决方案：
  1. 删除锁文件：`C:\Users\<用户名>\.gradle\wrapper\dists\gradle-8.5-bin\*\gradle-8.5-bin.zip.lck`
  2. 配置镜像源（可选）
  3. 使用 VPN 或代理

**问题：NDK 下载失败**
- 原因：自动下载 NDK 失败
- 解决：通过 Android Studio SDK Manager 手动安装 NDK

**问题：CMake 未找到**
- 原因：未安装 CMake
- 解决：通过 Android Studio SDK Manager 安装 CMake 3.22.1+

**问题：JDK 版本不兼容**
- 要求：JDK 17 或更高版本
- 检查：`java -version`
- 解决：升级 JDK

### 运行时问题

### Windows 端

**问题：找不到音频设备**
- 确保音频设备正常工作
- 检查 NAudio 是否正确安装
- 尝试重启应用

**问题：连接失败**
- USB：检查 USB 调试是否启用
- WiFi：确认设备在同一网络，防火墙未阻止
- 查看日志获取详细错误信息

### Android 端

**问题：OpenSL ES 初始化失败**
- 查看 Logcat 日志获取详细错误
- 尝试切换到 AudioTrack 模式
- 检查设备是否支持 OpenSL ES

**问题：音频卡顿**
- 尝试降低采样率（48000 → 44100）
- 检查网络延迟（WiFi 模式）
- 切换到 OpenSL ES 低延迟模式

**问题：声道映射错误**
- 确认 Android 设备支持对应声道数
- 检查音频输出设置（系统设置）
- 尝试其他声道配置

## 技术支持

### 日志收集

**Windows 端**：
- 查看应用内"日志"窗口
- 日志级别：Info、Debug、Error

**Android 端**：
```bash
adb logcat -s AudioShareService AudioShareOpenSL > audioshare.log
```

### 常用调试命令

```bash
# 查看 Android 设备
adb devices

# 查看应用版本
adb shell dumpsys package com.picapico.audioshare | grep versionName

# 安装 APK
adb install -r AudioShare.apk

# 启动应用
adb shell am start -n com.picapico.audioshare/.MainActivity

# 查看实时日志
adb logcat -s AudioShareService:V AudioShareOpenSL:V
```

## 性能优化建议

### Windows 端
- 使用有线网络代替 WiFi 降低延迟
- 关闭不必要的音频效果
- 选择适当的采样率（44100Hz 通常足够）

### Android 端
- 首选 OpenSL ES 模式获得最低延迟
- 关闭电池优化以保持后台运行
- 使用 USB 连接避免网络抖动

### 网络优化
- 使用 5GHz WiFi 频段
- 减少网络跳数（路由器直连）
- 避免网络拥塞时段

## 版本历史

### v2.0.1 (2025-12-08 安全性与稳定性增强)
- **安全性改进**：
  - 修复音频缓冲区无上限扩容的 OOM 风险，添加 2MB 上限保护
  - 增强输入验证，防止恶意数据包攻击
- **稳定性改进**：
  - 修复线程竞态条件，提升并发音频处理的可靠性
  - 优化端口查找逻辑，避免极端情况下的无限循环
  - 改进锁机制，减少潜在的死锁风险
- **代码质量**：
  - 增强错误日志记录，便于问题诊断
  - 添加边界检查，确保优雅降级

### v2.0 (2025-12-08)
- 新增多声道支持（Mono/2.1/4.0/5.1/7.1）
- 新增 OpenSL ES 低延迟音频 API
- 新增 AAudio 预留接口
- 优化音频性能和缓冲管理
- 完善错误处理和自动降级

### v1.x
- 基础立体声和左右声道支持
- WiFi 和 USB 连接
- Musiche 音乐播放器

