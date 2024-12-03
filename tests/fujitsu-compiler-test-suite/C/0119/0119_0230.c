#include <stdlib.h>
#include <math.h>
#include  <stdio.h>
int rz();
int ge();
int ne();
int gt();
int eq();
int le();
int lt();
unsigned data[] = {
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 1,         1,         1,
 1,         1,         1,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0,
 0,         0,         0
};
int check(i,j,k)
unsigned i;
unsigned j;
unsigned k;
{
    static int c=0;
    if( data[c++] != i ) {
      puts(" NG ");
      exit(1);
    }
    if( data[c++] != j ) {
      puts(" NG ");
      exit(1);
    }
    if( data[c++] != k ) {
      puts(" NG ");
      exit(1);
    }
    return 0;
}

unsigned char           i1dim1[] =
{ 0, 1, 0x7f,    0x80,    0x81 ,    0xff};
unsigned short      int i2dim1[] =
{ 0, 1 ,0x7fff,  0x8000,  0x8001 ,  0xffff };
unsigned            int i4dim1[] =
{ 0, 1, 0x7fffff,0x800000,0x800001 ,0xffffffff };
int main()
{
       lt(); le();
       eq(); ne();
       gt(); ge();
       rz();
       puts(" OK ");
exit (0);
}
int lt()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 < op1;
            i2ans[i] = i2 < op2;
            i4ans[i] = i4 < op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int le()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 <= op1;
            i2ans[i] = i2 <= op2;
            i4ans[i] = i4 <= op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int eq()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 == op1;
            i2ans[i] = i2 == op2;
            i4ans[i] = i4 == op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int ne()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 != op1;
            i2ans[i] = i2 != op2;
            i4ans[i] = i4 != op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int gt()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 >  op1;
            i2ans[i] = i2 >  op2;
            i4ans[i] = i4 >  op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int ge()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
       for(j=0;j<6;j++) {
          op1        = i1dim1[j];
          op2        = i2dim1[j];
          op4        = i4dim1[j];
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 >= op1;
            i2ans[i] = i2 >= op2;
            i4ans[i] = i4 >= op4;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
       }
       return 0;
}
int rz()
{
       int i,j;
       unsigned char  i1,i1ans[11],op1;
       unsigned short i2,i2ans[11],op2;
       unsigned int   i4,i4ans[11],op4;
         for(i=0;i<6;i++) {
            i1         = i1dim1[i];
            i2         = i2dim1[i];
            i4         = i4dim1[i];
            i1ans[i] = i1 == 0;
            i2ans[i] = i2 == 0;
            i4ans[i] = i4 == 0;
         }
         for(i=0;i<6;i++) {
           check(i1ans[i],
                                       i2ans[i],
                                       i4ans[i] );
         }
	 return 0;
}
