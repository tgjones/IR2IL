#include <stdlib.h>
#include <math.h>
#include <stdio.h>
int test3 (float h);
int test2 (int a, float b, int c, float d, int e, float f, int g, float h, int i);
int test1 (int a, int b, int c, int d, int e, int f, int g, int h, int i);
int main() {
int a,b,c ;
printf(" ***** mreg0005 ***** \n") ;

a = test1(1,2,3,4,5,6,7,8,9) ;

b = test2(1,2.0,3,4.0,5,6.0,7,8.0,9) ;

printf(" a = %d   b = %d \n",a,b) ;

exit (0);
}
int test1 (int a, int b, int c, int d, int e, int f, int g, int h, int i)
{
	int j,k,l ;

	if ( a > b ) {
		c = d + e ;
		goto L0 ;
	} else {
		j = h*i ;
	}
	f = test2(a,2.0,b,4.0,c,6.0,d,8.0,e) ;
	goto L3 ;
L0 :	goto L1 ;
L3 :	a = c + j ;
	goto L2 ;
	L1 : return(a) ;
	L2 : return(b) ;
}
int test2 (int a, float b, int c, float d, int e, float f, int g, float h, int i)
{
	if ( b-d > 2.0 ) {
		h = h+f ;
		a = test3(h) ;
		goto L1 ;
	} else {
		a = c + g ;
	}
	a = a + e ;
	L1 :
	return(a) ;
}
int test3 (float h)
{
	return(h) ;
}

