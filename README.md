# SnapShotPro

## 中文说明

### 项目简介

SnapShotPro 是一个基于 C# 和 WinForms 开发的轻量级 Windows 截图工具。它常驻系统托盘，支持区域截图、窗口截图、全屏截图，以及截图后的基础标注、复制和保存。

这个项目适合希望获得一个简单、直接、可本地修改的截图工具的开发者和用户。

### 主要功能

- 区域截图
- 窗口截图
- 全屏截图
- 延时截图（2 秒 / 5 秒）
- 全局快捷键触发
- 标注工具：箭头、矩形、文字、马赛克
- 撤销 / 重做
- 复制到剪贴板
- 保存为图片文件
- 托盘菜单与快捷键设置

### 默认快捷键

- `Alt + A`：区域截图
- `Alt + W`：窗口截图
- `Alt + F`：全屏截图

快捷键可以在程序的“快捷键设置”窗口中修改。

### 使用方法

#### 运行程序

可直接运行：

- `release/SnapShotPro.exe`

程序启动后会驻留在系统托盘。你可以通过以下方式使用它：

- 按默认快捷键开始截图
- 双击托盘图标开始区域截图
- 右键托盘图标，从菜单选择区域截图、窗口截图、全屏截图或延时截图

#### 截图后操作

截图完成后会打开编辑窗口，你可以：

- 添加箭头、矩形、文字或马赛克标注
- 使用撤销 / 重做调整内容
- 复制截图到剪贴板
- 保存截图到本地文件

### 配置文件

程序会在可执行文件同目录下生成配置文件：

- `SnapShotPro.ini`

配置项包括：

- 保存目录
- 图片格式
- 是否自动复制到剪贴板
- 自定义快捷键

### 开发与构建

项目源码位于：

- `src/SnapShotPro`

这是一个传统 WinForms 项目，目标框架为 `.NET Framework 4.0`。可使用 Visual Studio 打开 `src/SnapShotPro/SnapShotPro.csproj` 进行开发和构建。

当前仓库中也提供了可直接运行的发布版本：

- `release/SnapShotPro.exe`

### 项目结构

```text
docs/                设计与分析文档
release/             可直接运行的程序
src/SnapShotPro/     项目源码
```

---

## English

### Overview

SnapShotPro is a lightweight Windows screenshot tool built with C# and WinForms. It runs in the system tray and supports region capture, window capture, full-screen capture, and basic annotation after taking a screenshot.

This project is intended for users and developers who want a simple, hackable, local desktop screenshot utility.

### Features

- Region capture
- Window capture
- Full-screen capture
- Delayed capture (2s / 5s)
- Global hotkeys
- Annotation tools: arrow, rectangle, text, mosaic
- Undo / redo
- Copy to clipboard
- Save to image file
- Tray menu and hotkey settings

### Default Hotkeys

- `Alt + A`: Region capture
- `Alt + W`: Window capture
- `Alt + F`: Full-screen capture

Hotkeys can be changed in the app's hotkey settings dialog.

### How to Use

#### Run the app

You can launch the bundled executable directly:

- `release/SnapShotPro.exe`

After startup, the app stays in the system tray. You can use it by:

- pressing the default hotkeys
- double-clicking the tray icon to start a region capture
- right-clicking the tray icon and choosing a capture mode from the menu

#### After capturing

Once a screenshot is taken, the editor window opens. You can:

- add arrow, rectangle, text, or mosaic annotations
- use undo / redo
- copy the image to the clipboard
- save the image to a local file

### Configuration

The app stores its configuration in:

- `SnapShotPro.ini`

This file is created in the same directory as the executable and includes:

- save folder
- image format
- auto-copy option
- custom hotkeys

### Development and Build

Source code is located in:

- `src/SnapShotPro`

This is a classic WinForms project targeting `.NET Framework 4.0`. You can open `src/SnapShotPro/SnapShotPro.csproj` in Visual Studio to build or modify the project.

The repository also includes a ready-to-run build:

- `release/SnapShotPro.exe`

### Project Structure

```text
docs/                design and analysis notes
release/             runnable build
src/SnapShotPro/     source code
```
