# 更新说明

## 2025-12-08 更新（安全性与稳定性增强）

### Bug 修复
- **缓冲区安全**：修复 `TcpService.java` 中音频数据缓冲区无上限扩容的安全隐患，添加 2MB 上限保护，防止恶意或异常数据包导致 OOM。
- **线程安全**：修复 `Speaker.cs` 中 `OnAudioAvailable` 方法的竞态条件问题，确保连接状态检查在锁保护范围内，避免并发异常。
- **端口查找优化**：改进 `NetworkUtils.java` 的 `getFreePort` 方法，添加最大尝试次数限制（1000次），避免在极端情况下的无限循环。

### 编译兼容性修复
- **Java 17/21 要求明确**：由于Gradle构建脚本编译器限制，**不支持Java 25**，推荐使用JDK 17 LTS或21 LTS。
- **Gradle升级**：升级 Gradle 从 8.5 → 8.12，提升整体兼容性。
- **AGP 升级**：升级 Android Gradle Plugin 从 8.2.1 → 8.7.3，提升编译性能和稳定性。
- **CS0168 警告**：修复 `Speaker.cs:263` 处未使用的异常变量声明，消除编译警告。

### 编译问题诊断与解决
- **Class file major version 69 错误**：详细说明Java 25不兼容的根本原因和3种解决方案。
- **SDK location not found 错误**：自动创建 `local.properties` 文件，配置Android SDK路径。
- **环境配置指南**：添加JAVA_HOME设置、多版本Java共存、JDK下载链接、SDK路径查找等完整指引。
- **编译流程优化**：提供完整的故障排查步骤和快速诊断命令。

### 代码质量提升
- **错误处理**：增强缓冲区扩容时的错误日志记录，便于问题诊断。
- **线程同步**：优化音频数据处理流程中的锁粒度，提升并发性能。
- **健壮性**：添加边界检查，确保系统在异常输入时能够优雅降级。

### 文档改进
- **编译指南**：更新 `ttia.md`，添加完整的编译命令和故障排查说明。
- **Java版本说明**：添加 Java 版本兼容性矩阵和 Gradle 版本对应关系。
- **警告说明**：添加 NETSDK1138、Class file major version 等警告的详细说明和解决方案。
- **分类清晰**：将故障排查分为"编译问题"和"运行时问题"两大类。

## 2025-12-08 更新（Hotfix）

### 语言与 UI 体验
- **动态声道列表**：`Speaker` 现在会在系统语言切换后重新加载声道名称，避免下拉菜单固定为默认语言。
- **Mono 掩码修正**：Mono 预设改为使用 `CHANNEL_OUT_FRONT_CENTER`，确保 Android 端真正以单声道输出。
- **界面联动**：`MainWindow` 在启动和语言变更时都会刷新声道列表，保证多语言环境下的显示准确。

### 文档修订
- `ttia.md` 与本文件依旧保持 UTF-8 与原有版式，同时修正了 Native 库 ABI 支持说明，并将告警段落改为明确的 `⚠️` 提示。

## 2025-12-08 更新（v2.0）

### 多声道能力扩展
- **Windows 端**：音频管线改为输出完整 PCM 帧，新增通道描述与映射（Mono、2.1、4.0、5.1、7.1 等），支持声道上下混并确保 LFE/环绕声路由准确。
- **UI 改进**：现可直接选择多种声道模式，原左/右声道也被兼容保留，所有设备配置将自动迁移。
- **协议兼容**：Android 端依据通道掩码自动配置播放。
- **智能混音**：支持自动上混和下混，当设备声道数与音源不匹配时自动适配。

#### 支持的声道布局
| 布局 | 声道数 | 说明 |
|------|--------|------|
| Mono | 1 | 单声道（中置） |
| Stereo | 2 | 立体声（前左+前右） |
| Left/Right | 1 | 仅左声道或右声道 |
| 2.1 Surround | 3 | 立体声 + 低音炮 |
| 4.0 Surround | 4 | 前置左右 + 后置左右 |
| 5.1 Surround | 6 | 前置L/R/C + 低音炮 + 后置L/R |
| 7.1 Surround | 8 | 5.1 + 侧环绕L/R |

### 安卓音频 API 支持
- **新增音频输出抽象**：除了兼容的 AudioTrack 以外，提供 OpenSL ES（native）与 AAudio（使用低延迟 AudioTrack 作为过渡实现）两个选项。
- **UI 选择器**：Android App 顶部新增"输出 API"选择器，可随时切换并持久化到偏好设置。
- **OpenSL ES**：通过 JNI 与 C++ BufferQueue 实现低延迟播放，完整支持多声道音频。
- **AAudio 预留**：未来将接入原生 AAudio，当前版本使用低延迟 AudioTrack（PERFORMANCE_MODE_LOW_LATENCY）作为过渡。
- **自动降级**：如果首选 API 初始化失败，自动降级到 AudioTrack 以确保兼容性。

#### 音频 API 对比
| API | 延迟 | 兼容性 | 推荐场景 |
|-----|------|--------|----------|
| AudioTrack | 正常 | 全版本 | 旧设备兼容模式 |
| OpenSL ES | 低 | Android 2.3+ | 实时音频（默认） |
| AAudio | 最低 | Android 8.0+ | 未来支持（当前用低延迟AudioTrack） |

### 代码优化
- **性能优化**：移除 AudioTrack write 方法中不必要的 flush 调用，提升音频写入性能。
- **错误处理**：完善 OpenSL ES 初始化失败时的降级处理逻辑。
- **内存管理**：优化 C++ 缓冲区队列管理，使用 std::deque 实现高效的缓冲区复用。

### 其他变更
- README 与界面文案补充多声道/音频 API 说明。
- 新增 `ttia.md`（使用+打包说明）与本文件。
- 资源文件添加音频 API 相关字符串（arrays.xml 和 strings.xml）。
- Windows/Android 构建脚本未在本地执行成功：`dotnet build` 因缺少 .NET SDK 无法运行，`gradlew assembleDebug` 在下载 Gradle 期间超时（锁文件未释放）。命令已记录，待装好 SDK 后即可复现。

### 技术实现细节

#### Windows 端（C#）
- **AudioChannel.cs**：定义支持的声道枚举（None/Left/Right/Stereo/Mono/2.1/4.0/5.1/7.1）。
- **ChannelRole.cs**：定义声道角色（FrontLeft/FrontRight/FrontCenter/LowFrequency等）及默认映射。
- **ChannelPresets.cs**：声道预设配置，包含 Android 声道掩码映射。
- **ChannelLayoutConverter.cs**：声道布局转换核心，实现智能混音矩阵算法。
- **AudioManager.cs**：使用 NAudio WasapiLoopbackCapture 捕获系统音频，支持最高 8 声道。
- **Speaker.cs**：网络传输，发送采样率和声道掩码给 Android 端。

#### Android 端（Java + C++）
- **AudioApi.java**：音频 API 枚举（AUDIO_TRACK/OPENSL/AAUDIO）。
- **AudioOutputEngine.java**：音频输出引擎接口。
- **AudioTrackEngine.java**：AudioTrack 实现，支持低延迟模式（Android 8.0+）。
- **NativeOpenSLEngine.java**：OpenSL ES JNI 接口。
- **AudioOutputFactory.java**：音频引擎工厂，根据配置创建相应引擎。
- **opensl_player.cpp**：OpenSL ES C++ 实现，完整的声道掩码转换和缓冲队列管理。
- **TcpService.java**：TCP 服务，接收音频数据并使用选定的音频引擎播放。
- **MainActivity.java**：UI 集成，音频 API 选择器（Spinner）。

### 已知问题
- AAudio 当前使用低延迟 AudioTrack 作为占位实现，待后续版本接入原生 AAudio API。
- Android 8.0 以下设备选择 AAudio 会自动降级到 OpenSL ES。
