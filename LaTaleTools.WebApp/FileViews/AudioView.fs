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
    |> addIfNotEmpty (nameof tags.Title) tags.Title
    |> addIfNotEmpty (nameof tags.TitleSort) tags.TitleSort
    |> addIfNotEmpty (nameof tags.JoinedPerformers) tags.JoinedPerformers
    |> addIfNotEmpty (nameof tags.JoinedAlbumArtists) tags.JoinedAlbumArtists
    |> addIfNotEmpty (nameof tags.Album) tags.Album
    |> addIfNotEmpty (nameof tags.AlbumSort) tags.AlbumSort
    |> addIfNotEmpty (nameof tags.Comment) tags.Comment
    |> addIfNotEmpty (nameof tags.JoinedGenres) tags.JoinedGenres
    |> addIfNotEmpty (nameof tags.Copyright) tags.Copyright
    |> addIfNotEmpty (nameof tags.JoinedComposers) tags.JoinedComposers
    |> addIfNotEmpty (nameof tags.Conductor) tags.Conductor
    |> addIfNotEmpty (nameof tags.Grouping) tags.Grouping
    |> addIfNotEmpty (nameof tags.Lyrics) tags.Lyrics
    |> addIfNotEmpty (nameof tags.RemixedBy) tags.RemixedBy
    |> addIfNotEmpty (nameof tags.Subtitle) tags.Subtitle
    |> addIfNotEmpty (nameof tags.AmazonId) tags.AmazonId
    |> addIfNotEmpty (nameof tags.InitialKey) tags.InitialKey
    |> addIfNotEmpty (nameof tags.ISRC) tags.ISRC
    |> addIfNotEmpty (nameof tags.MusicBrainzArtistId) tags.MusicBrainzArtistId
    |> addIfNotEmpty (nameof tags.MusicBrainzDiscId) tags.MusicBrainzDiscId
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseArtistId) tags.MusicBrainzReleaseArtistId
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseCountry) tags.MusicBrainzReleaseCountry
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseGroupId) tags.MusicBrainzReleaseGroupId
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseId) tags.MusicBrainzReleaseId
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseStatus) tags.MusicBrainzReleaseStatus
    |> addIfNotEmpty (nameof tags.MusicBrainzReleaseType) tags.MusicBrainzReleaseType
    |> addIfNotEmpty (nameof tags.MusicBrainzTrackId) tags.MusicBrainzTrackId
    |> addIfNotEmpty (nameof tags.MusicIpId) tags.MusicIpId
    |> addIfNotEmpty (nameof tags.TagTypes) (String.Join(", ", tags.TagTypes))
    |> addIfPositive (nameof tags.Year) tags.Year
    |> addIfPositive (nameof tags.Track) tags.Track
    |> addIfPositive (nameof tags.TrackCount) tags.TrackCount
    |> addIfPositive (nameof tags.Disc) tags.Disc
    |> addIfPositive (nameof tags.DiscCount) tags.DiscCount
    |> addIfPositive (nameof tags.BeatsPerMinute) tags.BeatsPerMinute
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

