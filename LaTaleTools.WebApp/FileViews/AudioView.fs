module LaTaleTools.WebApp.FileViews.AudioView

open System
open Giraffe.ViewEngine
open LaTaleTools.WebApp.Route
open TagLib

let private tagsToList (tags: Tag) =
    let addIfNotEmpty (name: string) (value: string) list =
        if String.IsNullOrWhiteSpace(value) then list
        else (name, value) :: list
    let addIfPositive (name: string) (value: uint) list =
        if value > 0u then (name, value.ToString()) :: list
        else list
    
    List.empty
    |> addIfNotEmpty "Title" tags.Title
    |> addIfNotEmpty "TitleSort" tags.TitleSort
    |> addIfNotEmpty "JoinedPerformers" tags.JoinedPerformers
    |> addIfNotEmpty "JoinedAlbumArtists" tags.JoinedAlbumArtists
    |> addIfNotEmpty "Album" tags.Album
    |> addIfNotEmpty "AlbumSort" tags.AlbumSort
    |> addIfNotEmpty "Comment" tags.Comment
    |> addIfNotEmpty "JoinedGenres" tags.JoinedGenres
    |> addIfNotEmpty "Copyright" tags.Copyright
    |> addIfNotEmpty "JoinedComposers" tags.JoinedComposers
    |> addIfNotEmpty "Conductor" tags.Conductor
    |> addIfNotEmpty "Grouping" tags.Grouping
    |> addIfNotEmpty "Lyrics" tags.Lyrics
    |> addIfNotEmpty "RemixedBy" tags.RemixedBy
    |> addIfNotEmpty "Subtitle" tags.Subtitle
    |> addIfNotEmpty "AmazonId" tags.AmazonId
    |> addIfNotEmpty "InitialKey" tags.InitialKey
    |> addIfNotEmpty "ISRC" tags.ISRC
    |> addIfNotEmpty "MusicBrainzArtistId" tags.MusicBrainzArtistId
    |> addIfNotEmpty "MusicBrainzDiscId" tags.MusicBrainzDiscId
    |> addIfNotEmpty "MusicBrainzReleaseArtistId" tags.MusicBrainzReleaseArtistId
    |> addIfNotEmpty "MusicBrainzReleaseCountry" tags.MusicBrainzReleaseCountry
    |> addIfNotEmpty "MusicBrainzReleaseGroupId" tags.MusicBrainzReleaseGroupId
    |> addIfNotEmpty "MusicBrainzReleaseId" tags.MusicBrainzReleaseId
    |> addIfNotEmpty "MusicBrainzReleaseStatus" tags.MusicBrainzReleaseStatus
    |> addIfNotEmpty "MusicBrainzReleaseType" tags.MusicBrainzReleaseType
    |> addIfNotEmpty "MusicBrainzTrackId" tags.MusicBrainzTrackId
    |> addIfNotEmpty "MusicIpId" tags.MusicIpId
    |> addIfNotEmpty "TagTypes" (String.Join(", ", tags.TagTypes))
    |> addIfPositive "Year" tags.Year
    |> addIfPositive "Track" tags.Track
    |> addIfPositive "TrackCount" tags.TrackCount
    |> addIfPositive "Disc" tags.Disc
    |> addIfPositive "DiscCount" tags.DiscCount
    |> addIfPositive "BeatsPerMinute" tags.BeatsPerMinute
    |> List.rev

let audioPlayerComponent fullPath description (tags: Tag) = [
    audio [ _controls; _autoplay; _loop ] [
        source [ _src (pathToDownloadLink fullPath) ]
    ]
    
    br []
    b [] [ str "Metadata" ]
    br []
    table [] [
        yield tr [] [
            td [] [ str "Description" ]
            td [] [ str description ]
        ]
        yield!
            tagsToList tags
            |> List.map (fun (k, v) ->
                tr [] [
                    td [] [ str k ]
                    td [] [ str v ]
                ]
            )
    ]
]

