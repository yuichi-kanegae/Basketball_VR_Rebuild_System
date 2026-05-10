# Basketball VR Rebuild System

[![デモ動画](https://img.youtube.com/vi/qVROUS0B-wc/0.jpg)](https://youtu.be/qVROUS0B-wc)

## 概要
本プロジェクトは、バスケットボールの試合映像から抽出された選手の位置データ（JSON）をUnity上で同期し、VR空間内で試合展開を多角的に再現・分析するためのシステムです。
Meta Quest 2/3に対応しており、俯瞰視点での戦術確認だけでなく、特定の選手の一人称視点を追体験することで、意思決定の質を向上させるトレーニングツールとしての活用を想定しています。

## 主な機能
- **試合再現**: Python等の外部ツールで生成されたJSON形式の位置データを読み込み、3D空間にリアルタイムで反映。
- **マルチ視点切り替え**:
  - **俯瞰視点 (Overhead View)**: コート全体を把握するためのタクティカル視点。複数のプリセットカメラを瞬時に切り替え可能。
  - **一人称視点 (Player View)**: 選択した選手の視点にカメラを固定。選手がその瞬間に「何を見ていたか」を追体験。
- **直感的なVR操作 (UX)**:
  - 再生/一時停止、シーク、再生速度の動的変更。
  - レーザーポインターによる選手の直接選択。
  - 常に視界の正面に追従するダイナミックUIパネル。
- **データハンドリング**: 複数の試合データ（JSON）を、アプリケーションを再起動することなく動的に切り替え可能。

## 技術スタック
- **Engine**: Unity 2022.3.10f1
- **Target Device**: Meta Quest 2 / Meta Quest 3 (Standalone)
- **Language**: C#
- **Data Format**: JSON (Newtonsoft.Json / JsonUtility)

## プロジェクト構成（主要スクリプト）

| スクリプト名 | 役割 |
| :--- | :--- |
| `GameController.cs` | **System Core**: データの読み込み、オブジェクトの生成、シミュレーション時間の同期管理を担当。 |
| `VRInputController.cs` | **Input Manager**: コントローラー入力の検知、Raycastを用いた選手選択、各アクションのトリガー。 |
| `VRPlayerTracker.cs` | **Camera Control**: 俯瞰ポイントの管理および、ターゲット選手へのスムーズなカメラ追従。 |
| `VideoControlUI.cs` | **Interface**: 再生制御やデータ切り替えを行うためのUIイベントハンドリング。 |
| `UIFollowCamera.cs` | **UX Enhancement**: HMDの動きに合わせ、操作パネルを常にプレイヤーの正面の最適距離へ配置。 |

## 操作方法 (Controller Mapping)

| 入力 | 機能 | 詳細 |
| :--- | :--- | :--- |
| **Aボタン (右手)** | 再生 / 一時停止 | シミュレーションの進行をトグルで切り替え |
| **Bボタン (右手)** | 視点切り替え | 登録された複数の俯瞰視点ポイントを順送りに切り替え |
| **右トリガー** | 選手選択 | レーザーが指している選手を選択し、一人称視点へ移動 |
| **Xボタン (左手)** | UI表示切替 | メニューUIの表示/非表示を切り替え |
| **右スティック** | ポインター操作 | UI操作および対象の選択に使用 |

## データ仕様
読み込むJSONファイルは以下の構造を想定しています。

```json
[
  {
    "frame_index": 0,
    "players": [
      {
        "tracker_id": 1,
        "team_id": 0,
        "x": 32.35,
        "y": 26.16
      }
    ]
  }
]
```

## セットアップ
1. **JSONデータの配置**: `Resources` フォルダ、またはスクリプトで指定したデータパスに解析済みのJSONファイルを配置してください。
2. **Prefabのアサイン**: UnityのInspectorウィンドウで `GameController` を選択し、`Team1PlayerPrefab` および `Team2PlayerPrefab` に対応するプレイヤープリハブをそれぞれアサインしてください。
3. **俯瞰ポイントの設定**: `VRPlayerTracker` の `Overhead Transforms` リストに、コート全体を見渡せる位置に配置したGameObject（カメラの切り替え先）を複数登録してください。
4. **スケール調整**: コートのモデルサイズと位置データの単位を一致させるため、`GameController` の `Position Scale` の値を調整してください（デフォルト推奨値: 0.1）。
