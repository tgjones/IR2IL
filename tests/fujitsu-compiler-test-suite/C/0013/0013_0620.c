                                                             
#include <stdio.h>                                              

union SUB_UHB00HW00 {
	unsigned char m1;
	unsigned long m2;
};

struct GSHU00IW03HW00 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	unsigned long m3;
};

struct GSHU00IW03HW00 gshu00iw03hw00_0[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5, 6 }};
struct GSHU00IW03HW00 gshu00iw03hw00_1[2] = {{ { 1 }, 2, 3 }};
struct GSHU00IW03HW00 gshu00iw03hw00_2[2] = {{ { 1 }, 2 },{ { 4 }, 5, 6 }};
struct GSHU00IW03HW00 gshu00iw03hw00_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03HW00 gshu00iw03hw00_4[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5 }};

struct GSHU00IW03IW03 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	unsigned long m3:3;
};

struct GSHU00IW03IW03 gshu00iw03iw03_0[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5, 6 }};
struct GSHU00IW03IW03 gshu00iw03iw03_1[2] = {{ { 1 }, 2, 3 }};
struct GSHU00IW03IW03 gshu00iw03iw03_2[2] = {{ { 1 }, 2 },{ { 4 }, 5, 6 }};
struct GSHU00IW03IW03 gshu00iw03iw03_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03IW03 gshu00iw03iw03_4[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5 }};

struct GSHU00IW03IW29 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	unsigned long m3:29;
};

struct GSHU00IW03IW29 gshu00iw03iw29_0[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5, 6 }};
struct GSHU00IW03IW29 gshu00iw03iw29_1[2] = {{ { 1 }, 2, 3 }};
struct GSHU00IW03IW29 gshu00iw03iw29_2[2] = {{ { 1 }, 2 },{ { 4 }, 5, 6 }};
struct GSHU00IW03IW29 gshu00iw03iw29_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03IW29 gshu00iw03iw29_4[2] = {{ { 1 }, 2, 3 },{ { 4 }, 5 }};

struct SUB_SHB00HW00 {
	unsigned char m1;
	unsigned long m2;
};

struct GSHU00IW03HS00 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	struct SUB_SHB00HW00 m3;
};

struct GSHU00IW03HS00 gshu00iw03hs00_0[2] = {{ { 1 }, 2, { 3, 4 } },{ { 5 }, 6, { 7, 8 } }};
struct GSHU00IW03HS00 gshu00iw03hs00_1[2] = {{ { 1 }, 2, { 3, 4 } }};
struct GSHU00IW03HS00 gshu00iw03hs00_2[2] = {{ { 1 }, 2 },{ { 5 }, 6, { 7, 8 } }};
struct GSHU00IW03HS00 gshu00iw03hs00_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03HS00 gshu00iw03hs00_4[2] = {{ { 1 }, 2, { 3 } },{ { 5 }, 6, { 7, 8 } }};
struct GSHU00IW03HS00 gshu00iw03hs00_5[2] = {{ { 1 }, 2, { 3 } }};
struct GSHU00IW03HS00 gshu00iw03hs00_8[2] = {{ { 1 }, 2, { 3, 4 } },{ { 5 }, 6 }};
struct GSHU00IW03HS00 gshu00iw03hs00_16[2] = {{ { 1 }, 2, { 3, 4 } },{ { 5 }, 6, { 7 } }};

struct GSHU00IW03HU00 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	union SUB_UHB00HW00 m3;
};

struct GSHU00IW03HU00 gshu00iw03hu00_0[2] = {{ { 1 }, 2, { 3 } },{ { 4 }, 5, { 6 } }};
struct GSHU00IW03HU00 gshu00iw03hu00_1[2] = {{ { 1 }, 2, { 3 } }};
struct GSHU00IW03HU00 gshu00iw03hu00_2[2] = {{ { 1 }, 2 },{ { 4 }, 5, { 6 } }};
struct GSHU00IW03HU00 gshu00iw03hu00_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03HU00 gshu00iw03hu00_4[2] = {{ { 1 }, 2 },{ { 4 }, 5 }};

struct GSHU00IW03HA01 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	unsigned char m3[3];
};

struct GSHU00IW03HA01 gshu00iw03ha01_0[2] = {{ { 1 }, 2, "34" },{ { 5 }, 6, "78" }};
struct GSHU00IW03HA01 gshu00iw03ha01_1[2] = {{ { 1 }, 2, "34" }};
struct GSHU00IW03HA01 gshu00iw03ha01_2[2] = {{ { 1 }, 2 },{ { 5 }, 6, "78" }};
struct GSHU00IW03HA01 gshu00iw03ha01_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03HA01 gshu00iw03ha01_4[2] = {{ { 1 }, 2, "3" },{ { 5 }, 6, "78" }};
struct GSHU00IW03HA01 gshu00iw03ha01_5[2] = {{ { 1 }, 2, "3" }};
struct GSHU00IW03HA01 gshu00iw03ha01_8[2] = {{ { 1 }, 2, "34" },{ { 5 }, 6 }};
struct GSHU00IW03HA01 gshu00iw03ha01_16[2] = {{ { 1 }, 2, "34" },{ { 5 }, 6, "7" }};

struct GSHU00IW03HA04 {
	union SUB_UHB00HW00 m1;
	unsigned long m2:3;
	unsigned long m3[2];
};

struct GSHU00IW03HA04 gshu00iw03ha04_0[2] = {{ { 1 }, 2, {3,4} },{ { 5 }, 6, {7,8} }};
struct GSHU00IW03HA04 gshu00iw03ha04_1[2] = {{ { 1 }, 2, {3,4} }};
struct GSHU00IW03HA04 gshu00iw03ha04_2[2] = {{ { 1 }, 2 },{ { 5 }, 6, {7,8} }};
struct GSHU00IW03HA04 gshu00iw03ha04_3[2] = {{ { 1 }, 2 }};
struct GSHU00IW03HA04 gshu00iw03ha04_4[2] = {{ { 1 }, 2, {3} },{ { 5 }, 6, {7,8} }};
struct GSHU00IW03HA04 gshu00iw03ha04_5[2] = {{ { 1 }, 2, {3} }};
struct GSHU00IW03HA04 gshu00iw03ha04_8[2] = {{ { 1 }, 2, {3,4} },{ { 5 }, 6 }};
struct GSHU00IW03HA04 gshu00iw03ha04_16[2] = {{ { 1 }, 2, {3,4} },{ { 5 }, 6, {7} }};

int main() {

	printf("%u\n"  , gshu00iw03hw00_0[0].m1.m1);
	printf("%d\n" , gshu00iw03hw00_0[0].m2);
	printf("%lu\n" , gshu00iw03hw00_0[0].m3);
	printf("%u\n"  , gshu00iw03hw00_0[1].m1.m1);
	printf("%d\n" , gshu00iw03hw00_0[1].m2);
	printf("%lu\n" , gshu00iw03hw00_0[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03hw00_1[0].m1.m1);
	printf("%d\n" , gshu00iw03hw00_1[0].m2);
	printf("%lu\n" , gshu00iw03hw00_1[0].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03hw00_2[0].m1.m1);
	printf("%d\n" , gshu00iw03hw00_2[0].m2);
	printf("%u\n"  , gshu00iw03hw00_2[1].m1.m1);
	printf("%d\n" , gshu00iw03hw00_2[1].m2);
	printf("%lu\n" , gshu00iw03hw00_2[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03hw00_3[0].m1.m1);
	printf("%d\n" , gshu00iw03hw00_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hw00_4[0].m1.m1);
	printf("%d\n" , gshu00iw03hw00_4[0].m2);
	printf("%lu\n" , gshu00iw03hw00_4[0].m3);
	printf("%u\n"  , gshu00iw03hw00_4[1].m1.m1);
	printf("%d\n" , gshu00iw03hw00_4[1].m2);
	printf("\n");

	printf("%u\n"  , gshu00iw03iw03_0[0].m1.m1);
	printf("%d\n" , gshu00iw03iw03_0[0].m2);
	printf("%d\n" , gshu00iw03iw03_0[0].m3);
	printf("%u\n"  , gshu00iw03iw03_0[1].m1.m1);
	printf("%d\n" , gshu00iw03iw03_0[1].m2);
	printf("%d\n" , gshu00iw03iw03_0[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw03_1[0].m1.m1);
	printf("%d\n" , gshu00iw03iw03_1[0].m2);
	printf("%d\n" , gshu00iw03iw03_1[0].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw03_2[0].m1.m1);
	printf("%d\n" , gshu00iw03iw03_2[0].m2);
	printf("%u\n"  , gshu00iw03iw03_2[1].m1.m1);
	printf("%d\n" , gshu00iw03iw03_2[1].m2);
	printf("%d\n" , gshu00iw03iw03_2[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw03_3[0].m1.m1);
	printf("%d\n" , gshu00iw03iw03_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw03_4[0].m1.m1);
	printf("%d\n" , gshu00iw03iw03_4[0].m2);
	printf("%d\n" , gshu00iw03iw03_4[0].m3);
	printf("%u\n"  , gshu00iw03iw03_4[1].m1.m1);
	printf("%d\n" , gshu00iw03iw03_4[1].m2);
	printf("\n");

	printf("%u\n"  , gshu00iw03iw29_0[0].m1.m1);
	printf("%d\n" , gshu00iw03iw29_0[0].m2);
	printf("%d\n" , gshu00iw03iw29_0[0].m3);
	printf("%u\n"  , gshu00iw03iw29_0[1].m1.m1);
	printf("%d\n" , gshu00iw03iw29_0[1].m2);
	printf("%d\n" , gshu00iw03iw29_0[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw29_1[0].m1.m1);
	printf("%d\n" , gshu00iw03iw29_1[0].m2);
	printf("%d\n" , gshu00iw03iw29_1[0].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw29_2[0].m1.m1);
	printf("%d\n" , gshu00iw03iw29_2[0].m2);
	printf("%u\n"  , gshu00iw03iw29_2[1].m1.m1);
	printf("%d\n" , gshu00iw03iw29_2[1].m2);
	printf("%d\n" , gshu00iw03iw29_2[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw29_3[0].m1.m1);
	printf("%d\n" , gshu00iw03iw29_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03iw29_4[0].m1.m1);
	printf("%d\n" , gshu00iw03iw29_4[0].m2);
	printf("%d\n" , gshu00iw03iw29_4[0].m3);
	printf("%u\n"  , gshu00iw03iw29_4[1].m1.m1);
	printf("%d\n" , gshu00iw03iw29_4[1].m2);
	printf("\n");

	printf("%u\n"  , gshu00iw03hs00_0[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_0[0].m2);
	printf("%u\n"  , gshu00iw03hs00_0[0].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_0[0].m3.m2);
	printf("%u\n"  , gshu00iw03hs00_0[1].m1.m1);
	printf("%d\n" , gshu00iw03hs00_0[1].m2);
	printf("%u\n"  , gshu00iw03hs00_0[1].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_0[1].m3.m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_1[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_1[0].m2);
	printf("%u\n"  , gshu00iw03hs00_1[0].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_1[0].m3.m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_2[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_2[0].m2);
	printf("%u\n"  , gshu00iw03hs00_2[1].m1.m1);
	printf("%d\n" , gshu00iw03hs00_2[1].m2);
	printf("%u\n"  , gshu00iw03hs00_2[1].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_2[1].m3.m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_3[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_4[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_4[0].m2);
	printf("%u\n"  , gshu00iw03hs00_4[0].m3.m1);
	printf("%u\n"  , gshu00iw03hs00_4[1].m1.m1);
	printf("%d\n" , gshu00iw03hs00_4[1].m2);
	printf("%u\n"  , gshu00iw03hs00_4[1].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_4[1].m3.m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_5[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_5[0].m2);
	printf("%u\n"  , gshu00iw03hs00_5[0].m3.m1);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_8[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_8[0].m2);
	printf("%u\n"  , gshu00iw03hs00_8[0].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_8[0].m3.m2);
	printf("%u\n"  , gshu00iw03hs00_8[1].m1.m1);
	printf("%d\n" , gshu00iw03hs00_8[1].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hs00_16[0].m1.m1);
	printf("%d\n" , gshu00iw03hs00_16[0].m2);
	printf("%u\n"  , gshu00iw03hs00_16[0].m3.m1);
	printf("%lu\n" , gshu00iw03hs00_16[0].m3.m2);
	printf("%u\n"  , gshu00iw03hs00_16[1].m1.m1);
	printf("%d\n" , gshu00iw03hs00_16[1].m2);
	printf("%u\n"  , gshu00iw03hs00_16[1].m3.m1);
	printf("\n");

	printf("%u\n"  , gshu00iw03hu00_0[0].m1.m1);
	printf("%d\n" , gshu00iw03hu00_0[0].m2);
	printf("%u\n"  , gshu00iw03hu00_0[0].m3.m1);
	printf("%u\n"  , gshu00iw03hu00_0[1].m1.m1);
	printf("%d\n" , gshu00iw03hu00_0[1].m2);
	printf("%u\n"  , gshu00iw03hu00_0[1].m3.m1);
	printf("\n");
	printf("%u\n"  , gshu00iw03hu00_1[0].m1.m1);
	printf("%d\n" , gshu00iw03hu00_1[0].m2);
	printf("%u\n"  , gshu00iw03hu00_1[0].m3.m1);
	printf("\n");
	printf("%u\n"  , gshu00iw03hu00_2[0].m1.m1);
	printf("%d\n" , gshu00iw03hu00_2[0].m2);
	printf("%u\n"  , gshu00iw03hu00_2[1].m1.m1);
	printf("%d\n" , gshu00iw03hu00_2[1].m2);
	printf("%u\n"  , gshu00iw03hu00_2[1].m3.m1);
	printf("\n");
	printf("%u\n"  , gshu00iw03hu00_3[0].m1.m1);
	printf("%d\n" , gshu00iw03hu00_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03hu00_4[0].m1.m1);
	printf("%d\n" , gshu00iw03hu00_4[0].m2);
	printf("%u\n"  , gshu00iw03hu00_4[1].m1.m1);
	printf("%d\n" , gshu00iw03hu00_4[1].m2);
	printf("\n");

	printf("%u\n"  , gshu00iw03ha01_0[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_0[0].m2);
	printf("%s\n"  , gshu00iw03ha01_0[0].m3);
	printf("%u\n"  , gshu00iw03ha01_0[1].m1.m1);
	printf("%d\n" , gshu00iw03ha01_0[1].m2);
	printf("%s\n"  , gshu00iw03ha01_0[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_1[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_1[0].m2);
	printf("%s\n"  , gshu00iw03ha01_1[0].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_2[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_2[0].m2);
	printf("%u\n"  , gshu00iw03ha01_2[1].m1.m1);
	printf("%d\n" , gshu00iw03ha01_2[1].m2);
	printf("%s\n"  , gshu00iw03ha01_2[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_3[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_4[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_4[0].m2);
	printf("%s\n"  , gshu00iw03ha01_4[0].m3);
	printf("%u\n"  , gshu00iw03ha01_4[1].m1.m1);
	printf("%d\n" , gshu00iw03ha01_4[1].m2);
	printf("%s\n"  , gshu00iw03ha01_4[1].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_5[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_5[0].m2);
	printf("%s\n"  , gshu00iw03ha01_5[0].m3);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_8[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_8[0].m2);
	printf("%s\n"  , gshu00iw03ha01_8[0].m3);
	printf("%u\n"  , gshu00iw03ha01_8[1].m1.m1);
	printf("%d\n" , gshu00iw03ha01_8[1].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha01_16[0].m1.m1);
	printf("%d\n" , gshu00iw03ha01_16[0].m2);
	printf("%s\n"  , gshu00iw03ha01_16[0].m3);
	printf("%u\n"  , gshu00iw03ha01_16[1].m1.m1);
	printf("%d\n" , gshu00iw03ha01_16[1].m2);
	printf("%s\n"  , gshu00iw03ha01_16[1].m3);
	printf("\n");

	printf("%u\n"  , gshu00iw03ha04_0[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_0[0].m2);
	printf("%lu\n" , gshu00iw03ha04_0[0].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_0[0].m3[1]);
	printf("%u\n"  , gshu00iw03ha04_0[1].m1.m1);
	printf("%d\n" , gshu00iw03ha04_0[1].m2);
	printf("%lu\n" , gshu00iw03ha04_0[1].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_0[1].m3[1]);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_1[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_1[0].m2);
	printf("%lu\n" , gshu00iw03ha04_1[0].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_1[0].m3[1]);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_2[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_2[0].m2);
	printf("%u\n"  , gshu00iw03ha04_2[1].m1.m1);
	printf("%d\n" , gshu00iw03ha04_2[1].m2);
	printf("%lu\n" , gshu00iw03ha04_2[1].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_2[1].m3[1]);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_3[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_3[0].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_4[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_4[0].m2);
	printf("%lu\n" , gshu00iw03ha04_4[0].m3[0]);
	printf("%u\n"  , gshu00iw03ha04_4[1].m1.m1);
	printf("%d\n" , gshu00iw03ha04_4[1].m2);
	printf("%lu\n" , gshu00iw03ha04_4[1].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_4[1].m3[1]);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_5[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_5[0].m2);
	printf("%lu\n" , gshu00iw03ha04_5[0].m3[0]);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_8[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_8[0].m2);
	printf("%lu\n" , gshu00iw03ha04_8[0].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_8[0].m3[1]);
	printf("%u\n"  , gshu00iw03ha04_8[1].m1.m1);
	printf("%d\n" , gshu00iw03ha04_8[1].m2);
	printf("\n");
	printf("%u\n"  , gshu00iw03ha04_16[0].m1.m1);
	printf("%d\n" , gshu00iw03ha04_16[0].m2);
	printf("%lu\n" , gshu00iw03ha04_16[0].m3[0]);
	printf("%lu\n" , gshu00iw03ha04_16[0].m3[1]);
	printf("%u\n"  , gshu00iw03ha04_16[1].m1.m1);
	printf("%d\n" , gshu00iw03ha04_16[1].m2);
	printf("%lu\n" , gshu00iw03ha04_16[1].m3[0]);
	printf("\n");

	return 0;
}
