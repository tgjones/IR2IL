#include <stdio.h>
int f00_result_chk(r00,
                   c00,f00,l00,d00,x00,s00,h00,i00
                  ,c01,f01,l01,d01,x01,s01,h01,i01
                  ,c02,f02,l02,d02,x02,s02,h02,i02
                  ,c03,f03,l03,d03,x03,s03,h03,i03
                  ,c04,f04,l04,d04,x04,s04,h04,i04
                  ,c05,f05,l05,d05,x05,s05,h05,i05
                  ,c06,f06,l06,d06,x06,s06,h06,i06
                  ,c07,f07,l07,d07,x07,s07,h07,i07
                  ,c08,f08,l08,d08,x08,s08,h08,i08)
long   r00;
float  f00,f01,f02,f03,f04,f05,f06,f07,f08;
double d00,d01,d02,d03,d04,d05,d06,d07,d08;
long
double x00,x01,x02,x03,x04,x05,x06,x07,x08;
long long
int    l00,l01,l02,l03,l04,l05,l06,l07,l08;
long
int    i00,i01,i02,i03,i04,i05,i06,i07,i08;
unsigned long long
int    h00,h01,h02,h03,h04,h05,h06,h07,h08;
char   c00,c01,c02,c03,c04,c05,c06,c07,c08;
short
int    s00,s01,s02,s03,s04,s05,s06,s07,s08;
{
  if (r00==      (  c00+f00+l00+d00+x00+s00+h00+i00
                  + c01+f01+l01+d01+x01+s01+h01+i01
                  + c02+f02+l02+d02+x02+s02+h02+i02
                  + c03+f03+l03+d03+x03+s03+h03+i03
                  + c04+f04+l04+d04+x04+s04+h04+i04
                  + c05+f05+l05+d05+x05+s05+h05+i05
                  + c06+f06+l06+d06+x06+s06+h06+i06
                  + c07+f07+l07+d07+x07+s07+h07+i07
                  + c08+f08+l08+d08+x08+s08+h08+i08) )
    return 1;
  else
    return 0;
}
int main(){
float  f00[10],f01[10],f02[10],f03[10],f04[10],f05[10],f06[10],f07[10],f08[10];
double d00[10],d01[10],d02[10],d03[10],d04[10],d05[10],d06[10],d07[10],d08[10];
long
double x00[10],x01[10],x02[10],x03[10],x04[10],x05[10],x06[10],x07[10],x08[10];
long long
int    l00[10],l01[10],l02[10],l03[10],l04[10],l05[10],l06[10],l07[10],l08[10];
long
int    i00[10],i01[10],i02[10],i03[10],i04[10],i05[10],i06[10],i07[10],i08[10];
short
int    s00[10],s01[10],s02[10],s03[10],s04[10],s05[10],s06[10],s07[10],s08[10];
char   c00[10],c01[10],c02[10],c03[10],c04[10],c05[10],c06[10],c07[10],c08[10];
unsigned long long
int    h00[10],h01[10],h02[10],h03[10],h04[10],h05[10],h06[10],h07[10],h08[10];
long i,f00_result,r00[10];

 for (i=0;i < 10;i++)  {
     f00[i]=f01[i]=f02[i]=f03[i]=f04[i]=f05[i]=f06[i]=f07[i]=f08[i]=1.0f;
     h00[i]=h01[i]=h02[i]=h03[i]=h04[i]=h05[i]=h06[i]=h07[i]=h08[i]=f00[i];
     d00[i]=d01[i]=d02[i]=d03[i]=d04[i]=d05[i]=d06[i]=d07[i]=d08[i]=h00[i];
     x00[i]=x01[i]=x02[i]=x03[i]=x04[i]=x05[i]=x06[i]=x07[i]=x08[i]=d00[i];
     l00[i]=l01[i]=l02[i]=l03[i]=l04[i]=l05[i]=l06[i]=l07[i]=l08[i]=x00[i];
     i00[i]=i01[i]=i02[i]=i03[i]=i04[i]=i05[i]=i06[i]=i07[i]=i08[i]=l00[i];
     s00[i]=s01[i]=s02[i]=s03[i]=s04[i]=s05[i]=s06[i]=s07[i]=s08[i]=i00[i];
     c00[i]=c01[i]=c02[i]=c03[i]=c04[i]=c05[i]=c06[i]=c07[i]=c08[i]=s00[i];
 }
  for (i=0;i < 10;i++) {
  r00[i]=  f00[i]+f01[i]+f02[i]+f03[i]+f04[i]+f05[i]+f06[i]+f07[i]+f08[i]
         + h00[i]+h01[i]+h02[i]+h03[i]+h04[i]+h05[i]+h06[i]+h07[i]+h08[i]
         + d00[i]+d01[i]+d02[i]+d03[i]+d04[i]+d05[i]+d06[i]+d07[i]+d08[i]
         + x00[i]+x01[i]+x02[i]+x03[i]+x04[i]+x05[i]+x06[i]+x07[i]+x08[i]
         + i00[i]+i01[i]+i02[i]+i03[i]+i04[i]+i05[i]+i06[i]+i07[i]+i08[i]
         + s00[i]+s01[i]+s02[i]+s03[i]+s04[i]+s05[i]+s06[i]+s07[i]+s08[i]
         + l00[i]+l01[i]+l02[i]+l03[i]+l04[i]+l05[i]+l06[i]+l07[i]+l08[i]
         + c00[i]+c01[i]+c02[i]+c03[i]+c04[i]+c05[i]+c06[i]+c07[i]+c08[i];
 }
  for (i=0;i < 10;i++)  {
   f00_result=
   f00_result_chk( r00[i]
                  ,c00[i],f00[i],l00[i],d00[i],x00[i],s00[i],h00[i],i00[i]
                  ,c01[i],f01[i],l01[i],d01[i],x01[i],s01[i],h01[i],i01[i]
                  ,c02[i],f02[i],l02[i],d02[i],x02[i],s02[i],h02[i],i02[i]
                  ,c03[i],f03[i],l03[i],d03[i],x03[i],s03[i],h03[i],i03[i]
                  ,c04[i],f04[i],l04[i],d04[i],x04[i],s04[i],h04[i],i04[i]
                  ,c05[i],f05[i],l05[i],d05[i],x05[i],s05[i],h05[i],i05[i]
                  ,c06[i],f06[i],l06[i],d06[i],x06[i],s06[i],h06[i],i06[i]
                  ,c07[i],f07[i],l07[i],d07[i],x07[i],s07[i],h07[i],i07[i]
                  ,c08[i],f08[i],l08[i],d08[i],x08[i],s08[i],h08[i],i08[i]);
   if (f00_result) 
      printf ("**  (%d) ** OK\n",i+1);
   else {
      printf ("**  (%d) ** NG  ",i+1);
      printf("%d\n",(long)r00[i]);
   }
  }
}