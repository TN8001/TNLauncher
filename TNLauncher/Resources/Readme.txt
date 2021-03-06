﻿当ソフトはGoogle Chromeのお気に入りから、URIスキームを使ってデスクトップアプリケーションを起動するソフトです。（ブラウザがアプリランチャになります）


注意点がありますので最後まで読んで、納得された上でご利用ください。

・当ソフトはURIスキームをWindowsに登録するため、レジストリを使用します。（HKCU\SOFTWARE\Classes\tn-launcher-********\）
・十分安全性を考慮していますが、絶対安全とは言えません。URIスキームがばれる・ユーザーの設定ミス・当ソフトのバグ等で被害があっても、当方は一切責任は持ちません。（↓の説明をよく読んでください）
上記2点をご了承ください。

URIスキームとは、「http:」や「mailto:」等の「○○:」といったものです。Windowsではそれぞれのスキームにあったプログラムを起動する仕組みがあります。
「mailto:」をクリックすれば既定のメールソフトが起動します。
ブラウザのアドレス欄に「msnweather:」と入れると天気アプリが起動します。（検索にならないように注意）
Windowsストアにあるアプリ(UWP)の中には独自のURIスキームを持ったものが多くあります。
これ自体はWindowsとブラウザの機能ですので危険性はありません。
ｗｅｂページに電卓を開くリンクを作ったところで、驚く人はいるかもしれませんが、開くだけなので害はありません。

しかし任意のコマンドが実行できるとなると、話は全く変わります。情報の抜き取りやＰＣの破壊等あらゆる危険があります。

当ソフトはURIスキームで起動して、スキームに続くコマンドを実行し、自身は即終了します。（常駐ソフトではありません）
危険なあるいは安全なコマンドの選別は不可能と思われるので、当ソフトでは選別はしません。
そのかわりURIスキームの方を工夫して、狙い撃ちのリンクを作りにくいようにしています。

例えば悪意を持った人がｗｅｂページにＰＣの全ファイルを削除するコマンドのリンクを作っておいたとして、それをクリックした時に実際に削除される可能性は13京分の1です。（小文字の英数字11文字=36の11乗 デフォルト設定での場合 もっと強度を上げることは可能です）

仕組み上自分でｗｅｂページにリンクを作ることはできますが、たとえ非公開な場所でもやめてください。URIスキームがネットワーク上に平文で流れてしまいます。（ローカルのhtmlに作る場合はＯＫです）
