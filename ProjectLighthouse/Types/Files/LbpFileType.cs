namespace LBPUnion.ProjectLighthouse.Types.Files;

public enum LbpFileType
{
    Script, // .ff, FSH
    Texture, // TEX
    Level, // LVL
    CrossLevel, // PRF, Cross controller level
    FileArchive, // .farc, (ends with FARC)
    Plan, // PLN, uploaded with levels
    Voice, // VOP, voice data
    MotionRecording, // used in LBP2+/V for the motion recorder
    Painting, // PTG, paintings
    Jpeg, // JFIF / FIF, used in sticker switches,
    Png, // used in LBP Vita
    Unknown,
}