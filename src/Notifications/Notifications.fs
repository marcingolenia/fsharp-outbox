module Notifications

type Marker = interface end

type WhateverHappened =
  { Id: int64
    SomeText: string
    Amount: decimal }
  
type CrashTestsWhateverHappened =
  { Id: int64
    RefId: int
    Amount: decimal }

type InvoiceIssued = {
    Id: int64
    Number: string
    Amount: decimal
}

type InvoiceNotifications =
    | InvoiceIssued of InvoiceIssued