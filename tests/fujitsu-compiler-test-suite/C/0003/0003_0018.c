#if defined(__aarch64__) && defined(__ARM_FEATURE_SVE)
void dummy() ;
#include <arm_sve.h>
void test_rbit_z()
{
svint8_t vi1;
svint16_t vi2;
svint32_t vi4;
svint64_t vi8;
svuint8_t vu1;
svuint16_t vu2;
svuint32_t vu4;
svuint64_t vu8;
svfloat32_t vr4;
svfloat64_t vr8;
svbool_t pg, pg1, pg2, pg3, pg4, pg5, pg6;
svbool_t pg7, pg8, pg9, pg10, pg11, pg12;
svint8_t i1a,i1b,i1c;
svint16_t i2a,i2b,i2c;
svint32_t i4a,i4b,i4c;
svint64_t i8a,i8b,i8c;
svuint8_t u1a,u1b,u1c;
svuint16_t u2a,u2b,u2c;
svuint32_t u4a,u4b,u4c;
svuint64_t u8a,u8b,u8c;
svfloat32_t r4a,r4b,r4c;
svfloat64_t r8a,r8b,r8c;
signed char i1;
signed short i2;
signed int i4;
signed long long i8;
unsigned char u1;
unsigned short u2;
unsigned int u4;
unsigned long long u8;
float r4;
double r8;
  vi1 = svrbit_s8_z( pg,i1a);
  vi2 = svrbit_s16_z( pg,i2a);
  vi4 = svrbit_s32_z( pg,i4a); 
  vi8 = svrbit_s64_z( pg,i8a);
  dummy(vi1,	vi2,	vi4,	vi8);
  vu1 = svrbit_u8_z( pg,u1a);
  vu2 = svrbit_u16_z( pg,u2a);
  vu4 = svrbit_u32_z( pg,u4a); 
  vu8 = svrbit_u64_z( pg,u8a);
  dummy(vu1,	vu2,	vu4,	vu8);
}
void test_rbit_m()
{
svint8_t vi1;
svint16_t vi2;
svint32_t vi4;
svint64_t vi8;
svuint8_t vu1;
svuint16_t vu2;
svuint32_t vu4;
svuint64_t vu8;
svfloat32_t vr4;
svfloat64_t vr8;
svbool_t pg, pg1, pg2, pg3, pg4, pg5, pg6;
svbool_t pg7, pg8, pg9, pg10, pg11, pg12;
svint8_t i1a,i1b,i1c;
svint16_t i2a,i2b,i2c;
svint32_t i4a,i4b,i4c;
svint64_t i8a,i8b,i8c;
svuint8_t u1a,u1b,u1c;
svuint16_t u2a,u2b,u2c;
svuint32_t u4a,u4b,u4c;
svuint64_t u8a,u8b,u8c;
svfloat32_t r4a,r4b,r4c;
svfloat64_t r8a,r8b,r8c;
signed char i1;
signed short i2;
signed int i4;
signed long long i8;
unsigned char u1;
unsigned short u2;
unsigned int u4;
unsigned long long u8;
float r4;
double r8;
  vi1 = svrbit_s8_m( i1a,pg,i1b);
  vi2 = svrbit_s16_m( i2a,pg,i2b);
  vi4 = svrbit_s32_m( i4a,pg,i4b); 
  vi8 = svrbit_s64_m( i8a,pg,i8b);
  vu1 = svrbit_u8_m( u1a,pg,u1b);
  vu2 = svrbit_u16_m( u2a,pg,u2b);
  dummy(vi1,	vi2,	vi4,	vi8);
  vu4 = svrbit_u32_m( u4a,pg,u4b);
  vu8 = svrbit_u64_m( u8a,pg,u8b);
  dummy(vu1,	vu2,	vu4,	vu8);
}
void test_rbit_x()
{
svint8_t vi1;
svint16_t vi2;
svint32_t vi4;
svint64_t vi8;
svuint8_t vu1;
svuint16_t vu2;
svuint32_t vu4;
svuint64_t vu8;
svfloat32_t vr4;
svfloat64_t vr8;
svbool_t pg, pg1, pg2, pg3, pg4, pg5, pg6;
svbool_t pg7, pg8, pg9, pg10, pg11, pg12;
svint8_t i1a,i1b,i1c;
svint16_t i2a,i2b,i2c;
svint32_t i4a,i4b,i4c;
svint64_t i8a,i8b,i8c;
svuint8_t u1a,u1b,u1c;
svuint16_t u2a,u2b,u2c;
svuint32_t u4a,u4b,u4c;
svuint64_t u8a,u8b,u8c;
svfloat32_t r4a,r4b,r4c;
svfloat64_t r8a,r8b,r8c;
signed char i1;
signed short i2;
signed int i4;
signed long long i8;
unsigned char u1;
unsigned short u2;
unsigned int u4;
unsigned long long u8;
float r4;
double r8;
  vi1 = svrbit_s8_x( pg,i1a);
  vi2 = svrbit_s16_x( pg,i2a);
  vi4 = svrbit_s32_x( pg,i4a); 
  vi8 = svrbit_s64_x( pg,i8a);
  vu1 = svrbit_u8_x( pg,u1a);
  vu2 = svrbit_u16_x( pg,u2a);
  vu4 = svrbit_u32_x( pg,u4a); 
  vu8 = svrbit_u64_x( pg,u8a);
  dummy(vi1,	vi2,	vi4,	vi8,	vu1,	vu2,	vu4,	vu8);
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
