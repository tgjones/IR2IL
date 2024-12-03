#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
void dummy() ;
#include <arm_sve.h>
void test_rinta_z()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svrinta_f32_z( pg, r4a );
  vr8 = svrinta_f64_z( pg, r8a );
  dummy(vr4,vr8);
}
void test_rinta_m()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svrinta_f32_m( r4a, pg, r4b );
  vr8 = svrinta_f64_m( r8a, pg, r8b );
  dummy(vr4,vr8);
}
void test_rinta_x()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svrinta_f32_x( pg, r4a );
  vr8 = svrinta_f64_x( pg, r8a );
  dummy(vr4,vr8);
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
