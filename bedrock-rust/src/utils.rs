pub fn hash_string_vm(str: &str) -> u64 {
    return fnv1a_64(str.as_bytes());
}
pub fn hash_u64_vm(num: u64) -> u64 {
    return fnv1a_64(&num.to_le_bytes());
}
pub fn hash_blob_vm(data: &[u8]) -> u64 {
    return fnv1a_64(data);
}

pub fn read_u64(data: &[u8], offset: usize) -> u64 {
    let bytes = &data[offset..offset + 8];
    u64::from_le_bytes(bytes.try_into().unwrap())
}
pub fn write_u64(data: &mut [u8], offset: usize, value: u64) {
    let bytes = value.to_le_bytes();
    data[offset..offset + 8].copy_from_slice(&bytes);
}

pub fn read_string(data: &[u8], offset: usize, len: usize) -> String {
    let bytes = &data[offset..offset + len];
    String::from_utf8(bytes.to_vec()).unwrap()
}
pub fn write_string(data: &mut [u8], offset: usize, s: &str) {
    let bytes = s.as_bytes();
    data[offset..offset + bytes.len()].copy_from_slice(bytes);
}

fn fnv1a_64(data: &[u8]) -> u64 {
    const FNV_OFFSET_BASIS: u64 = 0xcbf29ce484222325;
    const FNV_PRIME: u64 = 0x00000100000001B3;

    let mut hash = FNV_OFFSET_BASIS;
    for &byte in data {
        hash ^= byte as u64;
        hash = hash.wrapping_mul(FNV_PRIME);
    }
    hash
}
