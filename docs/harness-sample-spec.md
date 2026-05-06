# HarnessSample 仕様書

## 概要

`HarnessSample` は Microsoft Agent Framework の Harness 系 AIContextProvider を使い、LM Studio の OpenAI 互換エンドポイントに接続して動作確認できる .NET 10 コンソールサンプルです。

このサンプルでは以下を確認します。

- LM Studio 接続設定を既存コードのまま再利用する
- `TodoProvider` によるタスク管理
- `AgentModeProvider` による `plan` / `execute` モード管理
- `SubAgentsProvider` によるサブエージェント委譲
- `FileMemoryProvider` による結果の Markdown 保存
- セッション状態の可視化
- 入力例付きの対話ループ

## 対象範囲

- 対象プロジェクト: `src/HarnessSample/HarnessSample.csproj`
- 主な対象ファイル:
    - `src/HarnessSample/Program.cs`
    - `src/HarnessSample/HarnessSampleConfiguration.cs`
    - `src/HarnessSample/WebSearchToolConfiguration.cs`
- 出力先: `bin/.../agent-files/` 配下

## 接続要件

既存コードの LM Studio 設定をそのまま利用します。

- Endpoint: `http://localhost:1234/v1`
- Model: `openai/gpt-oss-20b`
- API Key: `sk-dummy`

## Web 検索設定

- `TAVILY_API_KEY` または `TINYFISH_API_KEY` を設定すると Web 検索 Tool を有効化できる
- 設定ソースの優先順位は「OS 環境変数 → リポジトリ直下の `.env` → `scripts/.env`」とする
- `.env` はローカル開発補助であり、既存の環境変数を上書きしない
- `provider=auto` は Tavily を優先し、Tavily 未設定時のみ TinyFish を使う

## 実行シナリオ

1. コンソール起動時に入力例を表示し、調査テーマを受け付ける
2. plan モードで簡単な作業計画、TODO、サブエージェント利用方針を依頼する
3. execute モードで複数サブエージェントへ並列委譲し、結果整理と Markdown 保存を依頼する
4. 実行後に応答本文、TODO 一覧、StateBag、保存ファイル内容を表示する
5. ユーザーが `exit` を入力するまで対話ループを継続する

## 構成

```mermaid
flowchart TD
    U[User] --> P[HarnessSample Program]
    P --> O[OpenAI Compatible Client]
    O --> L[LM Studio]
    P --> A[ChatClientAgent]
    A --> T[TodoProvider]
    A --> M[AgentModeProvider]
    A --> SA[SubAgentsProvider]
    SA --> S1[OverviewResearchAgent]
    SA --> S2[ImplementationAdviceAgent]
    SA --> S3[ReviewChecklistAgent]
    A --> F[FileMemoryProvider]
    F --> S[Local File Store]
    A --> R[AgentSession StateBag]
```

## シーケンス

```mermaid
sequenceDiagram
    participant User
    participant Program
    participant Agent
    participant Providers as Harness Providers
    participant SubAgents
    participant LMStudio
    participant FileStore

    User->>Program: テーマ入力
    Program->>Agent: plan 用プロンプト送信
    Agent->>Providers: Todo / Mode / SubAgents コンテキスト取得
    Agent->>LMStudio: 推論要求
    LMStudio-->>Agent: 応答
    Agent-->>Program: 計画結果
    Program->>Agent: execute 用プロンプト送信
    Agent->>SubAgents: 複数サブタスクを並列開始
    SubAgents-->>Agent: 調査結果を返却
    Agent->>Providers: Todo / FileMemory 更新
    Agent->>LMStudio: 推論要求
    LMStudio-->>Agent: 応答
    Agent->>FileStore: harness-result.md 保存
    Program-->>User: 応答 / TODO / 保存結果を表示
```

## 非機能要件

- 追加ライブラリは既存参照を優先する
- 実装は単一ファイル中心で最小変更に留める
- LM Studio 未起動時は例外内容を表示し、接続確認ポイントを案内する
- サブエージェントは同一 LM Studio 接続を再利用する

## 完了条件

- `dotnet build` が成功する
- LM Studio 稼働時に `HarnessSample` が実行できる
- `.env` または環境変数設定時に起動表示で Web 検索 Tool が `enabled` になる
- 実行結果として TODO 状態、サブエージェント結果、保存ファイル確認ができる
