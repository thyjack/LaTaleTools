module LaTaleTools.Library.Encoding

open System.Text

do
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

let public filePathEncoding = Encoding.GetEncoding 949
let public ldtStringEncoding = Encoding.GetEncoding 936
let public xmlStringEncoding = Encoding.Unicode