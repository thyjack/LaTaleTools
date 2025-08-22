module LaTaleTools.WebApp.Route

open LaTaleTools.Library.Util

let browseBasePath = "/root"
let rawBasePath = "/raw"

let pathToBrowseLink path = $"{browseBasePath}/{trimPath path}"
let pathToDownloadLink path = $"{rawBasePath}/{trimPath path}"