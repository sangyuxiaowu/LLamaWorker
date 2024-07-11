![](doc/logo.png)

# LLamaWorker

LLamaWorkerは、[LLamaSharp](https://github.com/SciSharp/LLamaSharp?wt.mc_id=DT-MVP-5005195)プロジェクトに基づいて開発されたHTTP APIサーバーです。OpenAI互換のAPIを提供し、開発者が大規模言語モデル（LLM）を自分のアプリケーションに簡単に統合できるようにします。

英語 | [中文](README_CN.md)

## 特徴

- **OpenAI API互換**: OpenAIのAPIに似たAPIを提供し、移行と統合を容易にします。
- **マルチモデルサポート**: 異なるシナリオのニーズに応じて、異なるモデルの設定と切り替えをサポートします。
- **ストリーミング応答**: 大規模な応答の処理効率を向上させるために、ストリーミング応答をサポートします。
- **埋め込みサポート**: さまざまな埋め込みモデルをサポートするテキスト埋め込み機能を提供します。
- **チャットテンプレート**: 一部の一般的なチャットテンプレートを提供します。
- **自動リリース**: ロードされたモデルの自動リリースをサポートします。
- **APIキー認証**: APIキー認証をサポートします。
- **Gradio UIデモ**: Gradio.NETに基づいたUIデモを提供します。

## クイックスタート

1. リポジトリをローカルにクローン
   ```bash
   git clone https://github.com/sangyuxiaowu/LLamaWorker.git
   ```
2. プロジェクトディレクトリに移動
   ```bash
   cd LLamaWorker
   ```
3. ニーズに応じてプロジェクトファイルを選択します。プロジェクトは3つのバージョンのプロジェクトファイルを提供します：
   - `LLamaWorker.Backend.Cpu`: CPU環境用。
   - `LLamaWorker.Backend.Cuda11`: CUDA 11を搭載したGPU環境用。
   - `LLamaWorker.Backend.Cuda12`: CUDA 12を搭載したGPU環境用。
   
   次のステップに進むために、環境に適したプロジェクトファイルを選択します。
   
4. 依存関係をインストール
   ```bash
   dotnet restore LLamaWorker.Backend.Cpu\LLamaWorker.Backend.Cpu.csproj
   ```
   CUDAバージョンを使用している場合は、プロジェクトファイル名を適宜置き換えます。
   
5. 設定ファイル `appsettings.json` を変更します。デフォルトの設定には一般的なオープンソースモデルの設定が含まれており、必要に応じてモデルファイルパス（`ModelPath`）を変更するだけです。
   
6. サーバーを起動
   ```bash
   dotnet run --project LLamaWorker.Backend.Cpu\LLamaWorker.Backend.Cpu.csproj
   ```
   CUDAバージョンを使用している場合は、プロジェクトファイル名を適宜置き換えます。

## APIリファレンス

LLamaWorkerは以下のAPIエンドポイントを提供します：

- `/v1/chat/completions`: チャット完了リクエスト
- `/v1/completions`: プロンプト完了リクエスト
- `/v1/embeddings`: 埋め込みの作成
- `/models/info`: モデルの基本情報を返す
- `/models/config`: 設定されたモデル情報を返す
- `/models/{modelId}/switch`: 指定されたモデルに切り替える

## Gradio UIデモ

このUIは[Gradio.NET](https://github.com/feiyun0112/Gradio.Net?wt.mc_id=DT-MVP-5005195)に基づいています。

以下のコマンドを実行してGradio UIデモを試すこともできます：

```bash
dotnet restore ChatUI\ChatUI.csproj
dotnet run --project ChatUI\ChatUI.csproj
```

その後、ブラウザを開いてGradio UIデモにアクセスします。

![](doc/ui.png)
