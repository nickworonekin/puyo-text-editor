# Puyo Text Editor
Puyo Text Editor allows you to modify text in various Puyo Puyo games by converting the various text files to XML and visa-versa. The resulting XML files can be edited in your favorite text editor.

## Requirements
* [.NET 5.0+ Runtime](https://dotnet.microsoft.com/download)

## Download
The latest release can be downloaded from the [Releases](https://github.com/nickworonekin/puyo-text-editor/releases) page.

## Usage
```
PuyoTextEditor [options] <files>...
```

### Options
`-f, --format <format>`

The format of the file to convert. Can be one of the following values:
* cnvrs-text
* mtx
* fnt
* fpd

When this option is not set, the format will be determined by the file extension of the input file.

`--fnt <path>`

Set the FNT font file to use when converting to and from MTX files. This option is required for MTX files used in the non-3DS versions of Puyo Puyo!! 20th Anniversary.

`--fpd <path>`

Set the FPD font file to use when converting to and from MTX files. This option is required for MTX files used in Puyo Puyo!! 15th Anniversary and Puyo Puyo 7.

`-o, --output <path>`

Set the output filename. If not set, defaults to the input filename with an appropiate extension.

### Arguments

`files`

The files to convert. If the file extension ends with .xml, the file will be converted to its specified format; otherwise, the file will be converted to XML.

## Supported games
The following games are supported across all platforms:
* Puyo Puyo! 15th Anniversary
* Puyo Puyo 7
* Puyo Puyo!! 20th Anniversary
* Puyo Puyo!! Quest
* Puyo Puyo Tetris
* Puyo Puyo Chronicle
* Puyo Puyo Champions
* Puyo Puyo Tetris 2

## License
Puyo Text Editor is licensed under the [MIT license](LICENSE.md).