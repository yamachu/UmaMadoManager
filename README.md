# UmaMadoManager

DMM Gamesで配信されているウマ娘プリティーダービーのウィンドウを自動でリサイズすることが出来るアプリケーションです。

## 特徴

* 縦画面、横画面ごとにリサイズを行うかの設定が行うことが出来ます。
* ウマ娘プリティーダービーが存在するディスプレイを基準に左寄せ、右寄せする形でウィンドウサイズを拡大することが出来ます。
* バックグラウンド時や最小化時にウマ娘プリティーダービーの音をMuteすることが出来ます。
* トレイアプリのためタスクバーを占有しません。

## 動作環境

Windows 10 x64で動作確認しています。
アーキテクチャ依存ライブラリを使用していないためWindows 7や8、またWindows 10 x86環境でも動作するかもしれません。

## 配布に関して

[Booth](https://yamachu.booth.pm/items/2811984)にてビルド済みのアプリケーションを配布しています。
GitHub Release上でも今後配布予定ですが、それまでは上記リンクからダウンロードしてください。

## 開発者向け情報

### 開発環境

.NET 5.0 SDK (https://dotnet.microsoft.com/download/dotnet/5.0) をインストールしている環境が必要です。
UmaMadoManager.Coreのみの開発であればmacOSでも開発が可能となっています。
UmaMadoManager.Windowsの開発を行う場合はWindows環境が必要です。

### Debugビルドなどを行う場合

厳密名を使用してアプリケーションに署名を行っているため、署名キーが必要となります。
UmaMadoManager.Windowsディレクトリ以下の`keyfile.dev.snk`を`keyfile.snk`にリネームを行うことでUmaMadoManager.Windowsのビルドを行うことが出来ます。

### リリース時のコマンド

```
$ dotnet publish -c Release
```
