; /***************************************************************
; *  Projekt: JaProjIP
; *  Plik: JaAsm.asm
; *  Semestr/Rok akademicki: Zimowy 2024/2025
; *  Autor: Igor Potoczny
; *  Wersja: 21.37
; *  Opis pliku:
; *      Zawiera g³ówn¹ procedurê ApplyFilter, wywo³ywan¹ z C#,
; *      s³u¿¹c¹ do realizacji efektu podstawowego filtra noktowizji.
; ***************************************************************/
.DATA
    align 16
    brightnessFactor dd 1.2, 1.2, 1.2, 1.2
    lumCoeff_b      dd 0.0722, 0.0722, 0.0722, 0.0722
    lumCoeff_g      dd 0.7152, 0.7152, 0.7152, 0.7152
    lumCoeff_r      dd 0.2126, 0.2126, 0.2126, 0.2126
    greenFactor_br  dd 0.4, 0.4, 0.4, 0.4
    greenFactor_g   dd 1.2, 1.2, 1.2, 1.2
    float255        dd 255.0, 255.0, 255.0, 255.0
    floatZero       dd 0.0, 0.0, 0.0, 0.0

.CODE

ApplyFilter PROC
    push    rbp
    mov     rbp, rsp
    sub     rsp, 64                     ; Miejsce na lokalny bufor
    and     rsp, -16                    ; Wyrównanie do 16 bajtów

    push    rbx
    push    rsi
    push    rdi
    push    r12
    push    r13
    push    r14
    push    r15

    mov     r14, rcx                    ; pixelBuffer
    mov     r13d, edx                   ; width
    mov     r12d, r8d                   ; height
    mov     r11d, r9d                   ; stride

    lea     r15, [rsp + 32]             ; lokalny bufor
    and     r15, -16                    ; wyrównanie do 16 bajtów

    xor     r10d, r10d                  ; y = 0

row_loop:
    cmp     r10d, r12d
    jge     done

    mov     rax, r10
    mul     r11
    mov     rsi, rax                    ; rsi = y * stride

    xor     ebx, ebx                    ; x = 0

pixel_block_loop:
    mov     eax, r13d
    sub     eax, ebx
    cmp     eax, 4
    jl      remainder_pixels

    lea     rdi, [r14 + rsi]            ; Adres bazowy wiersza
    mov     rax, rbx                    ; Kopiuje x do rax
    lea     rax, [rax + rax*2]          ; rax = x*2 + x = x*3
    add     rdi, rax                    ; offset x*3

    ; Zerowanie XMM
    pxor    xmm0, xmm0                  ; Blue
    pxor    xmm1, xmm1                  ; Green
    pxor    xmm2, xmm2                  ; Red

    ; £adowanie 4 pikseli do tymczasowego bufora na stosie
    xor     ecx, ecx
load_pixels:
    mov     eax, ecx        
    imul    eax, 3         ; mno¿enie przez 3 dla RGB
    
    ; Blue
    movzx   edx, byte ptr [rdi + rax]     ; Blue
    cvtsi2ss xmm3, edx
    mov     eax, ecx
    shl     eax, 2                        ; mno¿enie przez 4 (rozm. float)
    movss   dword ptr [r15 + rax], xmm3

    ; Green
    mov     eax, ecx
    imul    eax, 3
    movzx   edx, byte ptr [rdi + rax + 1] ; Green
    cvtsi2ss xmm3, edx
    mov     eax, ecx
    shl     eax, 2                        ; mno¿enie przez 4 (rozm. float)
    movss   dword ptr [r15 + 16 + rax], xmm3

    ; Red
    mov     eax, ecx
    imul    eax, 3
    movzx   edx, byte ptr [rdi + rax + 2] ; Red
    cvtsi2ss xmm3, edx
    mov     eax, ecx
    shl     eax, 2                        ; mno¿enie przez 4 (rozm. float)
    movss   dword ptr [r15 + 32 + rax], xmm3

    inc     ecx
    cmp     ecx, 4
    jl      load_pixels

    ; £adowanie z bufora do rejestrów XMM
    movups  xmm0, [r15]                 ; B
    movups  xmm1, [r15 + 16]            ; G
    movups  xmm2, [r15 + 32]            ; R

    ; liczenie luminancji
    movaps  xmm3, xmm0
    mulps   xmm3, [lumCoeff_b]
    movaps  xmm4, xmm1
    mulps   xmm4, [lumCoeff_g]
    movaps  xmm5, xmm2
    mulps   xmm5, [lumCoeff_r]

    addps   xmm3, xmm4
    addps   xmm3, xmm5

    ; jasnoœæ
    mulps   xmm3, [brightnessFactor]

    ; liczenie sk³adowych NV
    movaps  xmm4, xmm3
    mulps   xmm3, [greenFactor_br]      ; B/R
    mulps   xmm4, [greenFactor_g]       ; G

    ; ograniczanie wartoœci
    maxps   xmm3, [floatZero]
    minps   xmm3, [float255]
    maxps   xmm4, [floatZero]
    minps   xmm4, [float255]

    ; Zapis do bufora
    movups  [r15], xmm3
    movups  [r15 + 16], xmm4

    ; Zapis przetworzonych pikseli z powrotem
    xor     ecx, ecx
store_pixels:
    ; Pobieranie wartoœci z bufora
    mov     eax, ecx                    ; licznik
    shl     eax, 2                      ; mno¿enie przez 4 (float)
    movss   xmm0, dword ptr [r15 + rax]      ; B/R
    movss   xmm1, dword ptr [r15 + 16 + rax] ; G
    
    ; Obliczanie offsetu dla zapisu RGB
    mov     eax, ecx
    imul    eax, 3                      ; mno¿enie przez 3 (RGB)
    
    ; Zapis wartoœci do obrazu
    cvttss2si edx, xmm0
    mov     byte ptr [rdi + rax], dl          ; B
    
    cvttss2si edx, xmm1
    mov     byte ptr [rdi + rax + 1], dl      ; G
    
    cvttss2si edx, xmm0
    mov     byte ptr [rdi + rax + 2], dl      ; R

    inc     ecx
    cmp     ecx, 4
    jl      store_pixels

    add     ebx, 4
    jmp     pixel_block_loop

remainder_pixels:
    cmp     ebx, r13d
    jge     next_row

    lea     rdi, [r14 + rsi]
    mov     eax, ebx
    imul    eax, 3
    add     rdi, rax

    movzx   edx, byte ptr [rdi]         ; B
    cvtsi2ss xmm0, edx
    mulss   xmm0, dword ptr [lumCoeff_b]

    movzx   edx, byte ptr [rdi + 1]     ; G
    cvtsi2ss xmm1, edx
    mulss   xmm1, dword ptr [lumCoeff_g]

    movzx   edx, byte ptr [rdi + 2]     ; R
    cvtsi2ss xmm2, edx
    mulss   xmm2, dword ptr [lumCoeff_r]

    addss   xmm0, xmm1
    addss   xmm0, xmm2

    mulss   xmm0, dword ptr [brightnessFactor]

    movss   xmm1, xmm0
    mulss   xmm0, dword ptr [greenFactor_br]
    mulss   xmm1, dword ptr [greenFactor_g]

    maxss   xmm0, dword ptr [floatZero]
    minss   xmm0, dword ptr [float255]
    maxss   xmm1, dword ptr [floatZero]
    minss   xmm1, dword ptr [float255]

    cvttss2si edx, xmm0
    mov     byte ptr [rdi], dl          ; B
    cvttss2si edx, xmm1
    mov     byte ptr [rdi + 1], dl      ; G
    cvttss2si edx, xmm0
    mov     byte ptr [rdi + 2], dl      ; R

    inc     ebx
    jmp     remainder_pixels

next_row:
    inc     r10d
    jmp     row_loop

done:
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     rdi
    pop     rsi
    pop     rbx
    mov     rsp, rbp
    pop     rbp
    ret

ApplyFilter ENDP

END