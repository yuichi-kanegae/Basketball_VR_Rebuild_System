基本的なことに関してはスクリプトファイルをすべてAIに投げると答えてくれると思います。

VR Basketball Analysis System (VRバスケットボール分析システム)概要本プロジェクトは、バスケットボールの試合映像から抽出した選手位置データ（JSON）をUnity上で読み込み、VR空間で試合を再現・分析するシステムです。
Meta Quest 2/3を使用し、俯瞰視点での戦術確認や、選手個人の視点（一人称視点）でのプレイ追体験が可能です。
システム要件
Unity Version: [2022.3.10f]
Target Device: Meta Quest 2 / Meta Quest 3
Language: C#
主な機能試合データの再生・制御: JSONデータを読み込み、選手の動きを3D空間で再現。再生・一時停止、再生速度変更、シークバーによる時間操作が可能。
視点切り替え:俯瞰視点 (Overhead View): コート全体を見渡す視点。ボタン操作で複数のカメラ位置を順次切り替え可能。
一人称視点 (Player View): 特定の選手を選択し、その選手の視点に追従。UI追従機能: 操作パネル（UI）がプレイヤーの視界正面に常に追従。
データ切り替え: UI上のボタン操作で、読み込む試合データ（JSONファイル）を動的に変更可能。プロジェクト構成 (Scripts & Data)1. システム・制御系 (Assets/Scripts/)GameController.csシステムの核となるスクリプト。JSONデータの読み込み (jsonFileNames 配列で管理)。プレイヤープリハブ (team1PlayerPrefab, team2PlayerPrefab) の生成と座標更新。再生状態（Play/Pause）、再生速度、現在フレームの管理。座標変換ロジック (GetUnityPosition: スケール調整、XY/XZ平面変換)。VRInputController.csVRコントローラーの入力を検知し、各機能を実行。レーザーポインターによる選手選択（Raycast）。ボタン入力による再生操作、視点切り替え、UI表示切り替え。VRPlayerTracker.csカメラ（プレイヤー）の移動制御を担当。
ターゲット追尾モード: 選択した選手(targetPlayer)の位置へカメラをスムーズに移動。
俯瞰モード: 事前に登録された複数の俯瞰視点ポイント(overheadTransforms)を管理・切り替え。

2. データ定義 (Assets/Scripts/)JsonDataClasses.csJSONデータの構造を定義するクラス群。
FrameData: 1フレームごとのデータ（フレーム番号、選手リスト）。
PlayerData: 各選手のデータ（トラッカーID、チームID、XY座標）。
3. UI・インターフェース (Assets/Scripts/)VideoControlUI.csuGUI (Canvas) のイベントハンドラ。再生/停止ボタン、タイムスライダー、速度スライダー、データ切り替えボタンの操作を GameController に伝達。UIFollowCamera.csUIキャンバスを常にカメラの正面（指定距離）に配置し、追従させるスクリプト。
4. 
5. 4. 入力データ (Assets/Resources/Data/ 推奨)last3.json (例)Python等の解析ツールで生成された位置情報データ。操作方法 (Controller Mapping)VRInputController.cs の実装に基づく操作方法は以下の通りです。ボタン手 (Hand)機能詳細Aボタン (Primary)右手再生 / 一時停止試合の進行をトグル切り替えBボタン (Secondary)右手俯瞰視点の切り替え登録された複数の俯瞰カメラ位置を順送り (CycleOverheadView)トリガー (Trigger)右手選手選択 (決定)レーザーが当たっている選手を選択し、その視点へ移動Xボタン (Menu)左手UI表示 / 非表示メニューUIの表示・非表示を切り替え（視界の邪魔にならないようにするため）スティック(右手)レーザー操作UI操作や選手選択に使用データ仕様 (JSON Format)本システムで読み込むJSONファイル（例: last3.json）は以下の配列構造である必要があります。
   5. JSON[
  {
    "frame_index": 0,
    "players": [
      {
        "tracker_id": 1,
        "team_id": 0,
        "x": 32.35,
        "y": 26.16
      },
      {
        "tracker_id": 2,
        "team_id": 1,
        "x": 27.99,
        "y": 26.71
      }
      // ... 他の選手データ
    ]
  },
  // ... 次のフレームデータ
]
tracker_id: 選手個別の識別ID (int)team_id: 所属チームID (0 または 1)x, y: コート上の2次元座標 (float)セットアップ時の注意点JSONファイルの配置: GameController は Resources フォルダ、または StreamingAssets など指定のパスからJSONを読み込みます（実装に応じてパスを確認してください）。Prefabの割り当て: Inspector上で GameController に Team1PlayerPrefab および Team2PlayerPrefab を必ずアサインしてください。俯瞰視点の設定: VRPlayerTracker の overheadTransforms リストに、コートを見渡せる位置（空のGameObject等）を複数登録してください。これがないとBボタンを押しても視点が切り替わりません。スケール調整: コートの広さと座標データの単位が合わない場合は、GameController の Position Scale の値を変更してください（デフォルトは 0.1 に設定されています）。
