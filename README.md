# Codex Quota Widget

[下载安装版 EXE](https://github.com/lgeng1656-rgb/codex-quota-widget/releases/download/v1.1.0/CodexQuotaWidget-Setup-1.1.0.exe) · [下载免安装 ZIP](https://github.com/lgeng1656-rgb/codex-quota-widget/releases/download/v1.1.0/CodexQuotaWidget-Windows.zip) · [查看发布说明](https://github.com/lgeng1656-rgb/codex-quota-widget/releases/latest)

Windows 桌面额度卡片。提供安装版 EXE 和免安装 ZIP，无需 Node.js，也无需手动运行脚本。需要 Windows 10/11 和系统 .NET Framework 4.8。ARM Windows 暂未验证。

## 下载与使用

上方下载链接对应已发布的 v1.1.0。要试用双主题新拟态界面，请按本文的构建说明编译 main 分支。

推荐下载 CodexQuotaWidget-Setup-1.1.0.exe，安装向导可选择开机自启和桌面快捷方式。安装完成后启动即可显示卡片。默认安装到当前用户目录，不需要管理员权限；卸载可从 Windows 已安装的应用中进行。

免安装：解压 CodexQuotaWidget-Windows.zip，双击 CodexQuotaWidget.exe。

重要前提：每位使用者必须在自己的电脑安装 Codex 桌面客户端，并登录 ChatGPT 账户。无需先打开 Codex 窗口，但登录状态必须有效。仅用 API Key 或账号未提供对应额度窗口时，组件可能无法显示该额度。

## 功能

- 五小时与每周额度，剩余百分比、重置倒计时与完整日期时间。
- 时间统一为北京时间 UTC+8。
- 每分钟读取一次；失败后 15 秒重试，悬停底部提示查看原因。
- 默认跟随 Windows 主题，右上角可手动切换深色和浅色。
- 非按钮区域可拖动；右上角刷新与置顶；右键菜单退出。
- 桌面卡片不显示任务栏入口，常见 Dock 通常也不会列出该工具窗口。
- 深浅两套哑光新拟态界面。浅色面板为纯白，两条额度进度条统一使用纯蓝圆角填充。

## 账号与隐私

软件使用本机 Codex app-server 的 account/rateLimits/read 接口，不发起模型生成请求。分发文件不含作者账户、凭据、个人配置、缓存或额度。认证由使用者本机 Codex 管理。不要将自己的 Codex 登录文件一起分发。

## 限制

未代码签名，Windows 可能提示未知发布者。本软件不是 OpenAI 官方产品；图标含 OpenAI 标识，不代表官方关联。Codex 协议更新后可能需要适配。已在开发者电脑验证，其他 Windows 版本、缩放与多屏环境尚未全面验收。

## 构建

在 Windows 项目目录运行：powershell.exe -NoProfile -ExecutionPolicy Bypass -File build.ps1 -Package

作用：编译原生 EXE 和免安装 ZIP；正常会生成 dist/CodexQuotaWidget.exe 和 dist/CodexQuotaWidget-Windows.zip。

安装版使用 Inno Setup 6：在项目目录运行 ISCC.exe installer.iss，正常会生成 dist/CodexQuotaWidget-Setup-1.1.0.exe。

源码和发行文件均不需要配置 API Key。
