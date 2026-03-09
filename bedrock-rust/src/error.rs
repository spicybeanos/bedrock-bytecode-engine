pub enum RuntimeError {
    UnkownSyscallHash,
    InvalidAddress,
    OutOfBounds,
    NotOwnerOfBuffer,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum BuildErrorKind {
    UnkownInstruction,
    InvalidParameter,
    InvalidOrMissingParameter,
    FileSizeLT0,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct BuildError {
    kind: BuildErrorKind,
    line: Option<usize>,
}

impl BuildError {
    pub const fn new(kind: BuildErrorKind) -> Self {
        Self { kind, line: None }
    }

    pub fn at_line(mut self, line: usize) -> Self {
        if self.line.is_none() {
            self.line = Some(line);
        }
        self
    }

    pub const fn kind(&self) -> BuildErrorKind {
        self.kind
    }

    pub const fn line(&self) -> Option<usize> {
        self.line
    }
}

impl From<BuildErrorKind> for BuildError {
    fn from(kind: BuildErrorKind) -> Self {
        Self::new(kind)
    }
}

#[allow(non_upper_case_globals)]
impl BuildError {
    pub const UnkownInstruction: Self = Self::new(BuildErrorKind::UnkownInstruction);
    pub const InvalidParameter: Self = Self::new(BuildErrorKind::InvalidParameter);
    pub const InvalidOrMissingParameter: Self =
        Self::new(BuildErrorKind::InvalidOrMissingParameter);
    pub const FileSizeLT0: Self = Self::new(BuildErrorKind::FileSizeLT0);
}