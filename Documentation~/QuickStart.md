# Nodevel クイックスタートガイド (QuickStart)

本ガイドでは、Nodevel を導入してから最初のノベル会話シーンを画面に表示するまでの手順を5分で解説します。

---

## 1. シナリオグラフの作成

1. Unity エディタの **Project ウィンドウ** で右クリック。
2. `Create > Nodevel > Graph Asset` を選択し、名前を `SampleStory` とします。
3. Unity の上部メニューから `Window > Nodevel > Graph Editor` を開きます。
4. ウィンドウ上部の `Graph Asset` フィールドに先ほど作成した `SampleStory` をドラッグ＆ドロップ（またはダブルクリックで開く）。

```text
[ Start Node ] ──> [ Script Node ] ──> [ End Node ]
```

5. グラフの何もない場所で右クリックし、以下の3つのノードを作成します：
   - `Create Node > Start Node`
   - `Create Node > Script Node`
   - `Create Node > End Node`
6. 各ノードの `Out` ポートから `In` ポートへドラッグして接続します。
7. `Script Node` のテキストボックスに以下のスクリプトを入力します：

```text
@bg "school" fade=0.5
アリス: こんにちは！今日もいい天気だね。
{wait 0.5}放課後、一緒に図書室に行かない？
```

---

## 2. 演出コマンドの定義 (`[NovelCommand]`)

Unity の C# スクリプト内で、演出（背景切り替え、サウンド再生、画面振動など）を行うメソッドを定義し、`[NovelCommand]` 属性を付与します：

```csharp
using UnityEngine;
using Junya005.Nodevel;

public class GameDirector : MonoBehaviour
{
    [NovelCommand("bg")]
    public async Awaitable ChangeBackground(string bgName, float fade = 0.5f)
    {
        Debug.Log($"背景を '{bgName}' に変更 (フェード時間: {fade}秒)");
        // プロジェクト固有のフェード処理を呼び出し
        await Awaitable.WaitForSecondsAsync(fade);
    }
}
```

---

## 3. シーンでの実行 (`GraphRunner`)

`INovelTextPresenter` を実装したテキスト表示コンポーネントを用意し、`GraphRunner` でシナリオを実行します：

```csharp
using System.Threading;
using UnityEngine;
using Junya005.Nodevel;

public class NovelSceneController : MonoBehaviour
{
    [SerializeField] private NodevelGraph _graph;
    [SerializeField] private GameDirector _director;

    private async void Start()
    {
        var variables = new VariableStore();
        var commands = new CommandRegistry();

        // 演出コマンドを登録
        commands.RegisterTarget(_director);

        // テキスト表示コンポーネント（サンプル同梱のBasicTextPresenter等）
        var textPresenter = GetComponent<INovelTextPresenter>();

        var runner = new GraphRunner(variables, commands, textPresenter);

        // シナリオ実行
        await runner.ExecuteAsync(_graph, destroyCancellationToken);
    }
}
```

---

## 4. サンプルシーンの利用

パッケージには uGUI および TextMeshPro を用いた動くサンプルシーンが同梱されています：
- `Packages/com.junya005.nodevel/Samples~/BasicNovel`
プロジェクトにインポートして、すぐに動作をご確認いただけます。

