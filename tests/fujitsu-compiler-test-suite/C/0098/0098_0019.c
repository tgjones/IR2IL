#include <stdlib.h>
#include <math.h>
#include <stdio.h>

struct st1{
  float a[10];
  float b[10];
  float av[10];
} t1;
int main(){
	long int n=10,i;
	float x;
	for(i=0;i<10;i++){
	        t1.a[i]=1.0;
	        t1.b[i]=1.0;
	        t1.av[i]=1.0;
	}
	for(i=0;i<n;i++){
		x=t1.a[i]*t1.b[i];
		t1.a[i]=x-t1.a[i];
		if (x == 0.0) goto LBL_99;
		t1.av[i]=sqrt(fabs(x));
	}
LBL_99:
	;
	printf("  ## A(I)  ##\n");
	for(i=0;i<10;i++){
		printf(" a[%ld] => %f\n",i,t1.a[i]);
	}
	printf("  ## AV(I) ##\n");
	for(i=0;i<10;i++){
		printf(" av[%ld] => %f\n",i,t1.av[i]);
	}
	exit (0);
}
