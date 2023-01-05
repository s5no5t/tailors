module Tweed.Web.ViewModels

type TweedViewModel = {
    Content: string
}

type IndexViewModel = {
    Tweeds: TweedViewModel list
}
