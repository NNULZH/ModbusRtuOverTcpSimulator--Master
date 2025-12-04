# ModbusRtuOverTcpSimulator (WPF)

本项目通过 **TCP 模拟 Modbus RTU 行为**，用于学习 RTU 帧结构、逐字节发送时序和基本读写流程。  
⚠ **注意：这并非标准的 RTU-over-TCP 或 Modbus TCP 实现，仅用于学习与演示。**

您可能需要结合另一个项目一起参考，以完整理解此项目的 RTU 逻辑：  
 https://github.com/NNULZH/ModbusRtuOverTcpSimulator--SlaveSimulator

说明（About obj / NuGet / project metadata）

为避免仓库体积膨胀，本项目通过 .gitignore 忽略了 bin/、obj/ 等编译产物。
其中 obj 目录包含大量 NuGet 还原产物、自动生成的中间文件、项目构建缓存，这些都不会上传到 GitHub。
⚠ 项目所需的 NuGet 包信息保存在 .csproj 中，可通过 dotnet restore 自动恢复，不需要上传 obj 内容。
---

## English

This project simulates **Modbus RTU behavior over TCP**, demonstrating RTU frame structure, byte-level timing, and basic read/write handling.  
⚠ **Note: This is NOT the standard RTU-over-TCP or Modbus TCP implementation; it is for learning and demonstration only.**

You may want to use it together with the companion project to fully understand the RTU logic:  
 https://github.com/NNULZH/ModbusRtuOverTcpSimulator--SlaveSimulator
 
**To keep the repository clean, this project uses .gitignore to exclude build artifacts such as bin/ and obj/.**
The obj directory contains NuGet restore results, auto-generated build metadata, and intermediate cache files, which are intentionally not uploaded.
⚠ All required NuGet package information is stored in the .csproj file, and can be restored automatically via dotnet restore; the obj contents are not needed in the repository.
