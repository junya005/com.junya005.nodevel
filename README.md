# Nodevel (com.junya005.nodevel)

[![Unity 6000.3+](https://img.shields.io/badge/Unity-6000.3+-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Nodevel**（ノードベル / Node + Novel）は、Unity 6 向けに設計された**ヘッドレス型ノベルアドベンチャー開発支援パッケージ**です。

---

## 🌟 特徴

- **ヘッドレス・コアライブラリ特化**:
  UI（uGUI / UI Toolkit）、描画（2D / 3D / Live2D / Spine）、サウンドエンジンに一切依存せず、プロジェクト側のアーキテクチャに完全に適合します。
- **マクロ（GraphView）× ミクロ（Text DSL）のハイブリッド構成**:
  ストーリーの大枠分岐やフラグ判定は Unity エディタ上のノードグラフで視覚的に組み立て、会話文や演出コマンドはノード内のテキストDSLで素早く執筆できます。
- **Unity 6 Awaitable によるゼロ依存非同期設計**:
  サードパーティライブラリ（UniTask等）への依存ゼロ。`UnityEngine.Awaitable` と `CancellationToken` で堅牢に動作します。
- **属性ベースの演出コマンド登録 (`[NovelCommand]`)**:
  任意のC#メソッドに属性を付けるだけでコマンド化。初期化時にデリゲートを事前キャッシュし、実行時リフレクションを排除して IL2CPP/AOT 環境でも最高速で動作します。
- **リプレイ方式によるセーブ・ロード・ロールバック**:
  セーブデータは「ノードID・行番号・変数」のみの極小サイズ。高速シミュレーション復元により、直前行への瞬時巻き戻し（ロールバック）にも標準対応。
- **中〜大規模商業ADV対応**:
  スクリプトパース結果の事前シリアライズ、アトミック書き込みによるセーブ破損防止、定常時ゼロアロケーション。

---

## 📦 インストール方法 (UPM)

Unity Package Manager の **"Add package from git URL..."** から以下のURLを指定してインストールします：

```text
https://github.com/junya005/Nodevel.git?path=Packages/com.junya005.nodevel
```

---

## 🚀 クイックスタート

詳細なセットアップ手順は [QuickStart ガイド](Documentation~/QuickStart.md) をご覧ください。

### 1. ノードグラフの作成
1. Unity プロジェクトビューで右クリック → `Create > Nodevel > Graph Asset` を選択。
2. 作成したアセットをダブルクリックするか、`Window > Nodevel > Graph Editor` からグラフを開きます。
3. グラフ上で右クリックし、`StartNode`、`ScriptNode`、`EndNode` を作成して線で繋ぎます。
4. `ScriptNode` のテキストエリアにシナリオを記述します：
   ```text
   @bg "classroom" fade=0.5
   アリス: こんにちは！今日もいい天気だね。
   {wait 0.5}ふふっ、何かいいことあった？
   ```

### 2. View層との接続
`INovelTextPresenter` を実装したコンポーネント（または同梱の `Samples~/BasicNovel`）を用意し、`GraphRunner` に渡して実行します：

```csharp
var variables = new VariableStore();
var commands = new CommandRegistry();
commands.RegisterTarget(this); // [NovelCommand] を登録

var runner = new GraphRunner(variables, commands, textPresenter, choicePresenter);
await runner.ExecuteAsync(myGraph, destroyCancellationToken);
```

---

## 📚 ドキュメント・アーキテクチャ

- **アーキテクチャ決定記録 (ADR)**: プロジェクトルートの `adr/` を参照。
- **API ドキュメント生成**: ルートで `doxygen Doxyfile` を実行すると、`Docs/Doxygen/html/index.html` に完全なAPIリファレンスが生成されます。

