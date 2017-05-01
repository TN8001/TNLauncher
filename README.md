# TNLauncher
![アプリスクリーンショット](https://github.com/TN8001/TNLauncher/blob/master/TNLauncher/AppImage.png)
## 概要
Google Chromeのお気に入りから、URIスキームを使ってデスクトップアプリケーションを起動するWindows用アプリケーションです。
## 特徴
* ブラウザのお気に入りから１クリックでアプリが立ち上がる
* ファビコンにアプリのアイコンが付くので一目でわかる
* Process.Startするだけなのでむちゃなコマンドも実行可能
* 安全性はランダムな英数字11桁をスキームに追加することで担保
## 使い方
1. 起動したい.exeまたは関連付けされたファイルをD&D
1. タイトル・コマンドライン等をを好みに変える
1. ファイル生成ボタンを押す
1. 出来たファイルをChromeのブックマークマネージャからインポートする
1. 「インポートされたブックマーク」フォルダから好みの位置に移動
以上
## 注意事項
* レジストリを使用します(HKCU\SOFTWARE\Classes\tn-launcher-********)
* 一切責任は持ちません
