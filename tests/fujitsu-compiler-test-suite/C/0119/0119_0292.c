#include <stdlib.h>
#include <math.h>
#include  <stdio.h>
int check( int i);

unsigned char           i1dim[] =
{ 0, 1, 0xff, 0x100, 0xffff, 0x10000, 0xffffffff, 0x100000000LL };
unsigned short      int i2dim[] =
{ 0, 1, 0xff, 0x100, 0xffff, 0x10000, 0xffffffff, 0x100000000LL };
unsigned            int i4dim[] =
{ 0, 1, 0xff, 0x100, 0xffff, 0x10000, 0xffffffff, 0x100000000LL };
int main()
{
       int i,sum1=0,sum2=0;
       unsigned char  i1,i1ans[8];
       unsigned short i2,i2ans[8];
       unsigned int   i4,i4ans[8];
       volatile unsigned char  i1b=0;
       volatile unsigned short i2b=0;
       volatile unsigned int   i4b=0;
       for(i=0;i<8;i++) {
          i1         = i1dim[i];
          i2         = i2dim[i];
          i4         = i4dim[i];
          i1b        = i1;
          i2b        = i2;
          i4b        = i4;
          i1ans[i] = i1b;
          i2ans[i] = i2b;
          i4ans[i] = i4b;
          sum1= i1+i2+i4+sum1;
          sum2= i1b+i2b+i4b+sum2;
       }
       for(i=0;i<8;i++) {
          if( check(i1ans[i]) ) puts("  NG  ");
          if( check(i2ans[i]) ) puts("  NG  ");
          if( check(i4ans[i]) ) puts("  NG  ");
       }
       puts(" OK ");
       sum1=sum1+1;
       sum2=sum2+1;         
exit (0);
}
unsigned int data[] = {
  0,         0,         0,
  1,         1,         1,
255,       255,       255,
  0,       256,       256,
255,     65535,     65535,
  0,         0,     65536,
255,     65535,4294967295,
  0,         0,         0
};
int check( int i)
{
   static int j=0;
   return i != data[j++];
}