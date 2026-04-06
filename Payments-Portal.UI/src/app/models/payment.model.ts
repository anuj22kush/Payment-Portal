export interface PaymentDto {
    id: string;
    reference: string;
    amount: number;
    currency: string;
    createdAt: string;
}

export interface CreatePaymentRequest {
    amount: number;
    currency: string;
    clientRequestId: string;
}

export interface UpdatePaymentRequest {
    amount: number;
    currency: string;
}