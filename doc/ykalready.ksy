meta:
  id: ykalready
  file-extension: dat
  endian: le

seq:
  - id: magic
    contents: ['YKALREADY01', 0]
  - id: index_count
    type: u4
  - id: index_offset
    type: u4
instances:
  sections:
    pos: index_offset
    type: section
    repeat: expr
    repeat-expr: index_count
types:
  section:
    seq:
      - id: name_offset
        type: u4
      - id: name_length
        type: u4
      - id: data_length
        type: u4
      - id: data_offset
        type: u4
    instances:
      name:
        pos: name_offset
        size: name_length
        type: str
        encoding: ascii
      data:
        pos: data_offset
        size: data_length