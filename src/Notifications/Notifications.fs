module Notifications

type Marker = interface end

type WhateverHappened =
  { Id: int64
    SomeText: string
    Amount: decimal }

type InvoiceIssued = {
    Id: int64
    Number: string
    Amount: decimal
}

type InvoiceNotifications =
    | InvoiceIssued of InvoiceIssued