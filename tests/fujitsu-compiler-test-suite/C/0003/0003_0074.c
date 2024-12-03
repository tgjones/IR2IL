#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
void dummy() ;
#include <arm_sve.h>
void test_cmpuo()
{
  svbool_t pg,pg1,pg2,pg3,pg4,pg5,pg6;
  svfloat32_t r4a,r4b;
  svfloat64_t r8a,r8b;
  float32_t r4;
  float64_t r8;
  pg1 = svcmpuo_f32( pg,r4a,r4b); 
  pg2 = svcmpuo_f64( pg,r8a,r8b);
  dummy(pg1,pg2);
}
void test_cmpuo_n()
{
  svbool_t pg,pg1,pg2,pg3,pg4,pg5,pg6;
  svfloat32_t r4a,r4b;
  svfloat64_t r8a,r8b;
  float32_t r4;
  float64_t r8;
  pg1 = svcmpuo_n_f32( pg,r4a,r4); 
  pg2 = svcmpuo_n_f64( pg,r8a,r8); 
  pg3 = svcmpuo_n_f32( pg,r4a,4); 
  dummy(pg1,pg2);
  pg4 = svcmpuo_n_f64( pg,r8a,8); 
  pg5 = svcmpuo_n_f32( pg,r4a,0); 
  pg6 = svcmpuo_n_f64( pg,r8a,0); 
  dummy(pg3,pg4,pg5,pg6);
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
