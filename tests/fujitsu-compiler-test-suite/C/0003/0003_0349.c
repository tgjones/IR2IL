#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
#include <arm_sve.h>

void test_s8(svbool_t pg,  int8_t *base)
{
  svint8x4_t data;
  svst4_vnum_s8(pg, base, 6, data);
}

void test_s16(svbool_t pg,  int16_t *base)
{
  svint16x4_t data;
  svst4_vnum_s16(pg, base, 6, data);
}

void test_s32(svbool_t pg,  int32_t *base)
{
  svint32x4_t data;
  svst4_vnum_s32(pg, base, 6, data);
}

void test_s64(svbool_t pg,  int64_t *base)
{
  svint64x4_t data;
  svst4_vnum_s64(pg, base, 6, data);
}

void test_u8(svbool_t pg,  uint8_t *base)
{
  svuint8x4_t data;
  svst4_vnum_u8(pg, base, 6, data);
}

void test_u16(svbool_t pg,  uint16_t *base)
{
  svuint16x4_t data;
  svst4_vnum_u16(pg, base, 6, data);
}

void test_u32(svbool_t pg,  uint32_t *base)
{
  svuint32x4_t data;
  svst4_vnum_u32(pg, base, 6, data);
}

void test_u64(svbool_t pg,  uint64_t *base)
{
  svuint64x4_t data;
  svst4_vnum_u64(pg, base, 6, data);
}

void test_f16(svbool_t pg,  float16_t *base)
{
  svfloat16x4_t data;
  svst4_vnum_f16(pg, base, 6, data);
}

void test_f32(svbool_t pg,  float32_t *base)
{
  svfloat32x4_t data;
  svst4_vnum_f32(pg, base, 6, data);
}

void test_f64(svbool_t pg,  float64_t *base)
{
  svfloat64x4_t data;
  svst4_vnum_f64(pg, base, 6, data);
}

void test2_s8(svbool_t pg,  int8_t *base)
{
  svint8x4_t data;
  svst4_vnum_s8(pg, base, -24, data);
}

void test2_s16(svbool_t pg,  int16_t *base)
{
  svint16x4_t data;
  svst4_vnum_s16(pg, base, -24, data);
}

void test2_s32(svbool_t pg,  int32_t *base)
{
  svint32x4_t data;
  svst4_vnum_s32(pg, base, -24, data);
}

void test2_s64(svbool_t pg,  int64_t *base)
{
  svint64x4_t data;
  svst4_vnum_s64(pg, base, -24, data);
}

void test2_u8(svbool_t pg,  uint8_t *base)
{
  svuint8x4_t data;
  svst4_vnum_u8(pg, base, -24, data);
}

void test2_u16(svbool_t pg,  uint16_t *base)
{
  svuint16x4_t data;
  svst4_vnum_u16(pg, base, -24, data);
}

void test2_u32(svbool_t pg,  uint32_t *base)
{
  svuint32x4_t data;
  svst4_vnum_u32(pg, base, -24, data);
}

void test2_u64(svbool_t pg,  uint64_t *base)
{
  svuint64x4_t data;
  svst4_vnum_u64(pg, base, -24, data);
}

void test2_f16(svbool_t pg,  float16_t *base)
{
  svfloat16x4_t data;
  svst4_vnum_f16(pg, base, -24, data);
}

void test2_f32(svbool_t pg,  float32_t *base)
{
  svfloat32x4_t data;
  svst4_vnum_f32(pg, base, -24, data);
}

void test2_f64(svbool_t pg,  float64_t *base)
{
  svfloat64x4_t data;
  svst4_vnum_f64(pg, base, -24, data);
}
int main()
{
  return 0;
}
#else
int main() {
return 0;
}
#endif
