#include <stdlib.h>
#include <stdio.h>
#include <math.h>

void sub()
{
	int i,n[10],m[10];
	double a[10],b[10],a0,a2,a4,a6,a8;
	n[0]=99; 
	n[1]=99; 
	n[2]=1 ; 
	n[3]= 2 ; 
	n[4]=3 ;
	n[5]=4 ; 
	n[6]=5 ; 
	n[7]=6 ; 
	n[8]= 7 ; 
	n[9]=8 ;
	m[0]=99; 
	m[1]=99; 
	m[2]=2 ; 
	m[3]= 3 ; 
	m[4]=4 ;
	m[5]=5 ; 
	m[6]=6 ; 
	m[7]=7 ; 
	m[8]= 8 ; 
	m[9]=9 ;
	a[0]=1.; 
	a[1]=0.; 
	a[2]=3.; 
	a[3]=-5.; 
	a[4]=0.;
	a[5]=2.; 
	a[6]=4.; 
	a[7]=0.; 
	a[8]=-4.; 
	a[9]=2.;
	b[0]=2.; 
	b[1]=2.; 
	b[2]=1.; 
	b[3]=-2.; 
	b[4]=1.;
	b[5]=9.; 
	b[6]=3.; 
	b[7]=4.; 
	b[8]=-9.; 
	b[9]=1.;


	for (i=2;i<=9;i++)
		a[n[i]] = a[m[i]]+b[n[i]];

	a0=a[0]; 
	a2=a[2]; 
	a4=a[4]; 
	a6=a[6]; 
	a8=a[8];
	printf("%13.5g %13.5g %13.5g %13.5g %13.5g\n",a0,a2,a4,a6,a8);
}
int main()
{
	sub();
	exit (0);
}
