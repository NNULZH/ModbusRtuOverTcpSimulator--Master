# ModbusRtuOverTcpSimulator (WPF)

本项目通过 **TCP 模拟 Modbus RTU 行为**，用于学习 RTU 帧结构、逐字节发送时序和基本读写流程。  
⚠ **注意：这并非标准的 RTU-over-TCP 或 Modbus TCP 实现，仅用于学习与演示。**

您可能需要结合另一个项目一起参考，以完整理解本项目的 RTU 逻辑：  
https://github.com/NNULZH/ModbusRtuOverTcpSimulator--SlaveSimulator

---

## 项目说明（About obj / NuGet / project metadata）

为避免仓库体积膨胀，本项目通过 `.gitignore` 忽略了 `bin/`、`obj/` 等编译产物。  
`obj` 目录中包含大量 **NuGet 还原文件、自动生成的中间代码、编译缓存** —— 这些都没有必要上传。  

⚠ 所需的 NuGet 包全部记录在 `.csproj` 中，只需 `dotnet restore` 即可自动恢复，无需担心 `obj` 的缺失。

---

## 关于“写”功能（A tiny confession about Write functions）

本项目的 **轮询 / 读功能已完整实现**；  
但“写功能”嘛……  
> 嗯……它并没有被我单独封装成完整模块，纯属我懒了 😭（没错，就是这么诚实）

不过别担心：

- 主界面里提供了 **写单个寄存器 / 写单个线圈** 功能  
- "写单个线圈/寄存器"指令和"读"指令都是 **8 字节标准指令结构**  
- 只要你理解 RTU 基础，就完全可以正常使用  
- 添加到表格后右键即可发送 —— 完全够用（真的！）

---

## English

This project simulates **Modbus RTU behavior over TCP**, demonstrating RTU frame structure, byte-level timing, and basic read/write handling.  
⚠ **Note: This is NOT the standard RTU-over-TCP or Modbus TCP implementation; it is intended purely for learning and demonstration.**

You may want to use it together with the companion project to fully understand the RTU logic:   
https://github.com/NNULZH/ModbusRtuOverTcpSimulator--SlaveSimulator

---

### About obj / NuGet / project metadata

To keep the repository clean, `.gitignore` excludes build artifacts such as `bin/` and `obj/`.  
The `obj` folder contains **NuGet restore results, intermediate build files, and auto-generated metadata**, which are intentionally not included.

⚠ All necessary NuGet packages are listed in the `.csproj` file, and can be restored via `dotnet restore`.  
You don’t need any of the files inside `obj`.

---

### A tiny confession about Write functions

The **polling / read logic is fully implemented**, but the write module…  
> well, let’s say I didn’t finish it because I was lazy 😭

Still:

- The UI provides **Write Single Register** and **Write Single Coil**  
- Both use the standard **8-byte RTU command format**  
- If you understand RTU basics, you can use them without any issue  
- Add them to the table → right-click → send — done.

After all, the goal of this project is to **help you understand RTU, not drown you in code**.

