#include <stdlib.h>
#include <math.h>
#include <stdio.h>

unsigned long int l1[10],l2=1 ;
unsigned long int l3[10]={
		1,1,1,1,1,0,0,0,0,0};
unsigned long int ii=1 ;
int main(){
	long int i,i1,i2,i3,i4,i5,i6,i7,i8,i9 ;

	for(i=0;i< 10;i++){
		l3[i]=15;
	}
	printf("\n");
	i1=ii;
	i2=ii;
	i3=ii;
	i4=ii;
	i5=ii;
	i6=ii;
	i7=ii;
	i8=ii;
	i9=ii;
	for(i=0;i<10;i++){
		l1[i]=l2|l3[i] ;
	}
	i1=i1+1;
	i2=i2+1;
	i3=i3+1;
	i4=i4+1;
	i5=i5+1;
	i6=i6+1;
	i7=i7+1;
	i8=i8+1;
	i9=i9+1;
	for(i=0;i<10;i++){
		printf(" %ld[%ld] %ld %ld[%ld] %ld %ld %ld %ld %ld %ld %ld %ld %ld %ld\n",
		    l1[i],i,l2,l3[i],i,ii,i1,i2,i3,i4,i5,i6,i7,i8,i9);
	}
	exit (0);
}
 
