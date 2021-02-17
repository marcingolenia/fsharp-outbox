namespace WebHost

open Notifications

module Handlers =
  let printWhateverHappenedWithSmiley (notification: WhateverHappened) =
    async {
      printfn "%A" notification
      printfn ":)"
    }