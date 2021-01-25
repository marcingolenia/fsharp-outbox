module Toolbox

    open System
    open IdGen

    let generateId =
        let structure = IdStructure(byte 41, byte 10, byte 12)
        let options = IdGeneratorOptions(structure, DefaultTimeSource(DateTimeOffset.Parse "2020-10-01 12:30:00"))
        let generator = IdGenerator(666, options)
        generator.CreateId