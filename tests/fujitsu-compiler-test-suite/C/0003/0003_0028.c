#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
void dummy() ;
#include <arm_sve.h>
void test_mad_z()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_f32_z( pg, r4a, r4b, r4c);
  vr8 = svmad_f64_z( pg, r8a, r8b, r8c);
  dummy(vr4,vr8);
}
void test_mad_m()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_f32_m( pg, r4a, r4b, r4c);
  vr8 = svmad_f64_m( pg, r8a, r8b, r8c);
  dummy(vr4,vr8);
}
void test_mad_x()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_f32_x( pg, r4a, r4b, r4c);
  vr8 = svmad_f64_x( pg, r8a, r8b, r8c);
  dummy(vr4,vr8);
}
void test_mad_n_z()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_z( pg, r4a, r4b, r4);
  vr8 = svmad_n_f64_z( pg, r8a, r8b, r8);
  dummy(vr4,vr8);
}
void test_mad_n_m()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_m( pg, r4a, r4b, r4);
  vr8 = svmad_n_f64_m( pg, r8a, r8b, r8);
  dummy(vr4,vr8);
}
void test_mad_n_x()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_x( pg, r4a, r4b, r4);
  vr8 = svmad_n_f64_x( pg, r8a, r8b, r8);
  dummy(vr4,vr8);
}
void test_mad_num_z()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_z( pg, r4a, r4b, 4);
  vr8 = svmad_n_f64_z( pg, r8a, r8b, 8);
  dummy(vr4,vr8);
}
void test_mad_num_m()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_m( pg, r4a, r4b, 4);
  vr8 = svmad_n_f64_m( pg, r8a, r8b, 8);
  dummy(vr4,vr8);
}
void test_mad_num_x()
{
  svfloat32_t vr4;
  svfloat64_t vr8;
  svbool_t pg;
  svfloat32_t r4a,r4b,r4c;
  svfloat64_t r8a,r8b,r8c;
  float32_t r4;
  float64_t r8;
  vr4 = svmad_n_f32_x( pg, r4a, r4b, 4);
  vr8 = svmad_n_f64_x( pg, r8a, r8b, 8);
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
