#include  <stdio.h>


       float  f00[10],f01[10],f02[10],f03[10],f04[10],f05[10],f06[10],f07[10],f08[10],f09[10]
            , f10[10],f11[10],f12[10],f13[10],f14[10],f15[10],f16[10],f17[10],f18[10],f19[10]
            , f20[10],f21[10],f22[10],f23[10],f24[10],f25[10],f26[10],f27[10],f28[10],f29[10];
       double d00[10],d01[10],d02[10],d03[10],d04[10],d05[10],d06[10],d07[10],d08[10],d09[10]
            , d10[10],d11[10],d12[10],d13[10],d14[10],d15[10],d16[10],d17[10],d18[10],d19[10]
            , d20[10],d21[10],d22[10],d23[10],d24[10],d25[10],d26[10],d27[10],d28[10],d29[10];
  long double x00[10],x01[10],x02[10],x03[10],x04[10],x05[10],x06[10],x07[10],x08[10],x09[10]
            , x10[10],x11[10],x12[10],x13[10],x14[10],x15[10],x16[10],x17[10],x18[10],x19[10]
            , x20[10],x21[10],x22[10],x23[10],x24[10],x25[10],x26[10],x27[10],x28[10],x29[10];
int d00_result_chk(p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29)
        double     p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29;{
  if (p0==30.0 && p1 && p2 && p3 && p4 && p5 && p6 && p7 && p8 && p9
               && p10 && p11 && p12 && p13 && p14 && p15 && p16 && p17 && p18 && p19
               && p20 && p21 && p22 && p23 && p24 && p25 && p26 && p27 && p28 && p29)
    return 1;
  else
    return 0;
}
int f00_result_chk(p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29)
            float  p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29;{
  if (p0==30 && p1 && p2 && p3 && p4 && p5 && p6 && p7 && p8 && p9
               && p10 && p11 && p12 && p13 && p14 && p15 && p16 && p17 && p18 && p19
               && p20 && p21 && p22 && p23 && p24 && p25 && p26 && p27 && p28 && p29)
    return 1;
  else
    return 0;
}
int x00_result_chk(p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29)
      long double  p0 ,p1 ,p2 ,p3 ,p4 ,p5 ,p6 ,p7 ,p8 ,p9
                  ,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19
                  ,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29;{
  if (p0==30.0 && p1 && p2 && p3 && p4 && p5 && p6 && p7 && p8 && p9
               && p10 && p11 && p12 && p13 && p14 && p15 && p16 && p17 && p18 && p19
               && p20 && p21 && p22 && p23 && p24 && p25 && p26 && p27 && p28 && p29)
    return 1;
  else
    return 0;
}
int main(){
       int i,x00_result,f00_result,d00_result;
 for (i=0; i < 10; i++)  {
     f00[i]=f01[i]=f02[i]=f03[i]=f04[i]=f05[i]=f06[i]=f07[i]=f08[i]=f09[i]
   = f10[i]=f11[i]=f12[i]=f13[i]=f14[i]=f15[i]=f16[i]=f17[i]=f18[i]=f19[i]
   = f20[i]=f21[i]=f22[i]=f23[i]=f24[i]=f25[i]=f26[i]=f27[i]=f28[i]=f29[i]=1.0;
  }
  for (i=0; i < 10; i++)  {
     d00[i]=d01[i]=d02[i]=d03[i]=d04[i]=d05[i]=d06[i]=d07[i]=d08[i]=d09[i]
   = d10[i]=d11[i]=d12[i]=d13[i]=d14[i]=d15[i]=d16[i]=d17[i]=d18[i]=d19[i]
   = d20[i]=d21[i]=d22[i]=d23[i]=d24[i]=d25[i]=d26[i]=d27[i]=d28[i]=d29[i]=1.0;
  }
  for (i=0; i < 10; i++)  {
     x00[i]=x01[i]=x02[i]=x03[i]=x04[i]=x05[i]=x06[i]=x07[i]=x08[i]=x09[i]
   = x10[i]=x11[i]=x12[i]=x13[i]=x14[i]=x15[i]=x16[i]=x17[i]=x18[i]=x19[i]
   = x20[i]=x21[i]=x22[i]=x23[i]=x24[i]=x25[i]=x26[i]=x27[i]=x28[i]=x29[i]=1.0;
 }
  for (i=0; i < 10; i++) {
    f00[i]=  f00[i]+f01[i]+f02[i]+f03[i]+f04[i]+f05[i]+f06[i]+f07[i]+f08[i]+f09[i]
           + f10[i]+f11[i]+f12[i]+f13[i]+f14[i]+f15[i]+f16[i]+f17[i]+f18[i]+f19[i]
           + f20[i]+f21[i]+f22[i]+f23[i]+f24[i]+f25[i]+f26[i]+f27[i]+f28[i]+f29[i];
    d00[i]=  d00[i]+d01[i]+d02[i]+d03[i]+d04[i]+d05[i]+d06[i]+d07[i]+d08[i]+d09[i]
           + d10[i]+d11[i]+d12[i]+d13[i]+d14[i]+d15[i]+d16[i]+d17[i]+d18[i]+d19[i]
           + d20[i]+d21[i]+d22[i]+d23[i]+d24[i]+d25[i]+d26[i]+d27[i]+d28[i]+d29[i];
    x00[i]=  x00[i]+x01[i]+x02[i]+x03[i]+x04[i]+x05[i]+x06[i]+x07[i]+x08[i]+x09[i]
           + x10[i]+x11[i]+x12[i]+x13[i]+x14[i]+x15[i]+x16[i]+x17[i]+x18[i]+x19[i]
           + x20[i]+x21[i]+x22[i]+x23[i]+x24[i]+x25[i]+x26[i]+x27[i]+x28[i]+x29[i];
 }
  for (i=0; i < 10; i++)  {
   f00_result=
   f00_result_chk(f00[i],f01[i],f02[i],f03[i],f04[i],f05[i],f06[i],f07[i],f08[i],f09[i]
                , f10[i],f11[i],f12[i],f13[i],f14[i],f15[i],f16[i],f17[i],f18[i],f19[i]
                , f20[i],f21[i],f22[i],f23[i],f24[i],f25[i],f26[i],f27[i],f28[i],f29[i]);
   d00_result=
   d00_result_chk(d00[i],d01[i],d02[i],d03[i],d04[i],d05[i],d06[i],d07[i],d08[i],d09[i]
                , d10[i],d11[i],d12[i],d13[i],d14[i],d15[i],d16[i],d17[i],d18[i],d19[i]
                , d20[i],d21[i],d22[i],d23[i],d24[i],d25[i],d26[i],d27[i],d28[i],d29[i]);
   x00_result=
   x00_result_chk(x00[i],x01[i],x02[i],x03[i],x04[i],x05[i],x06[i],x07[i],x08[i],x09[i]
                , x10[i],x11[i],x12[i],x13[i],x14[i],x15[i],x16[i],x17[i],x18[i],x19[i]
                , x20[i],x21[i],x22[i],x23[i],x24[i],x25[i],x26[i],x27[i],x28[i],x29[i]);
  if (x00_result &&  f00_result && d00_result) 
      printf ("**  (%d) ** OK\n",i+1);
   else
      printf ("**  (%d) ** NG\n",i+1);
  }
}