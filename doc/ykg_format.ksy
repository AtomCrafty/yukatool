meta:
  id: ykg
  file-extension: ykg
  endian: le
  
seq:
  - id: magic
    contents: ['YKG000', 0, 0]
  - id: header_length
    type: u4
  - size: 28
  - id: color_offset
    type: u4
  - id: color_length
    type: u4
  - id: alpha_offset
    type: u4
  - id: alpha_length
    type: u4
  - id: frame_offset
    type: u4
  - id: frame_length
    type: u4

instances:
  color_data:
    pos: color_offset+4
    size: color_length-4
    if: color_length > 0
  alpha_data:
    pos: alpha_offset
    size: alpha_length
    if: alpha_length > 0
  frame_count:
    value: frame_length / 32
  frame_data:
    pos: frame_offset
    type: frame_data
    repeat: expr
    repeat-expr: frame_count
    if: frame_count > 0
    
types:
  frame_data:
    seq:
      - id: x
        type: u4
      - id: y
        type: u4
      - id: width
        type: u4
      - id: height
        type: u4
      - id: duration
        type: u4
      - id: frame_type
        type: u4
        enum: frame_type
      - id: unknown1
        type: u4
      - id: unknown2
        type: u4

enums:
  frame_type:
    0: end
    1: single
    2: loop
    4: animation