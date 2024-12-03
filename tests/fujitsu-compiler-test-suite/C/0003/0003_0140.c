#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
void dummy() ;
#include <arm_sve.h>
void test_ld1sb( )
{
  svint32_t vi4;
  svint64_t vi8;
  svuint32_t vu4;
  svuint64_t vu8;
  svuint32_t u32base1,u32base2, u32base3;
  svuint64_t u64base1,u64base2, u64base3;
  svbool_t pg;
  
  vi4 = svld1sb_gather_u32base_s32(  pg, u32base1);
  vi8 = svld1sb_gather_u64base_s64(  pg, u64base1);
  vu4 = svld1sb_gather_u32base_u32(  pg, u32base2);
  vu8 = svld1sb_gather_u64base_u64(  pg, u64base2);
  dummy(vi4,	vi8,	vu4,	vu8);
}
void dummy() {};

int main()
{
  return 0;
}
#else
int main() {
return 0;
}
#endif
