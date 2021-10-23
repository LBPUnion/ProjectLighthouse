namespace LBPUnion.ProjectLighthouse.Types.Files {
    public enum LbpFileType {
        Script, // .ff, FSH
        Texture, // TEX
        Level, // LVL
        FileArchive, // .farc, (ends with FARC)
        Plan, // PLN, uploaded with levels
        Voice, // VOP, voice data
        Unknown,
    }
}