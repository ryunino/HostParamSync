# HostParamSync

Revit の **ホスト関係**（例：壁にぶら下がるドア/窓）に対して、
**コピー元のパラメータ値**を**コピー先のパラメータ**へ自動同期するアドインです。

- 種類: **External Application (IExternalApplication)**
- UI: **WPF モデルレスウィンドウ**
- 自動同期: **Dynamic Model Update (IUpdater)**
- 動作確認環境: **Revit 2024.2 / .NET Framework 4.8 / Visual Studio 2022**

> MVPでは「壁の［コメント］→ ドア/窓の［コメント］」を同期対象にしています。  
> 仕組みが動いたら、他カテゴリ/他パラメータへ拡張できます。

---

## 機能

- リボン「**HostParamSync**」タブ → 「**Sync**」パネル → **HostParamSync** ボタンで UI を開く
- UIで以下を設定
  - コピー元（ホスト）：**壁**／パラメータ：**コメント**
  - ペースト先（ホスト先）：**ドア または 窓**／パラメータ：**コメント**
  - **同期（手動）** ボタン
  - **常時同期（IUpdater）ON/OFF** チェック
- 常時同期ON時は、**壁のコメントを変更すると即座にドア/窓のコメントへ反映**

---

## セットアップ

1. クローン
   ```bash
   git clone https://github.com/ryunino/HostParamSync.git
   cd HostParamSync
   ```

2. Visual Studio 2022 で `HostParamSync.sln` を開く

3. 参照追加（未設定の場合）
   - `RevitAPI.dll`
   - `RevitAPIUI.dll`
   > Revit 2024 のインストール先から「参照の追加」で指定。プロパティの **ローカルコピー = False**。

4. ターゲットフレームワークの確認  
   プロジェクトのプロパティ → **.NET Framework 4.8**

---

## ビルド

Visual Studio で **Debug** または **Release** ビルド。  
出力：`./bin/<Configuration>/HostParamSync.dll`

---

## Revit での起動方法

### A. Add-In Manager（手動実行）

1. Revit 起動 → **Add-Ins** タブ → **Add-In Manager (Manual)**  
2. **Load** → `HostParamSync.dll` を選択  
3. クラス名：`HostParamSync.Addin.ExtApp` を指定してロード  
4. リボンに **HostParamSync** タブが現れ、**HostParamSync** ボタンで UI を開けます

### B. .addin ファイルで通常起動

`%AppData%\Autodesk\Revit\Addins\2024\HostParamSync.addin` を作成（サンプル）:

```xml
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>HostParamSync</Name>
    <Assembly>C:\Users\<USER>\Documents\Revit\HostParamSync\bin\Debug\HostParamSync.dll</Assembly>
    <AddInId>8D83CEC5-B739-4ACD-A9DB-1BC78D5AF4D4</AddInId>
    <FullClassName>HostParamSync.Addin.ExtApp</FullClassName>
    <VendorId>HPSY</VendorId>
    <VendorDescription>Host parameter sync.</VendorDescription>
  </AddIn>
</RevitAddIns>
```

---

## 使い方

1. Revit で任意のプロジェクトを開く  
2. リボン **HostParamSync → HostParamSync** をクリック（UI が開きます）  
3. デフォルト選択のまま（壁→ドア/窓：コメント）
   - **同期（手動）**：今ある要素を一括で同期  
   - **常時同期（IUpdater）**：ONにすると、壁のコメント変更がドア/窓に自動反映

> Updater は「**壁カテゴリ**の**コメント**パラメータ変更」をトリガーに動作します。

---

## ライセンス

このリポジトリのライセンスは **MIT** を想定しています。
