import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { PaymentsService } from './payments.service';
import { PaymentDto, CreatePaymentRequest, UpdatePaymentRequest } from './models/payment.model';
import { environment } from '../environments/environment';

describe('PaymentsService', () => {
    let service: PaymentsService;
    let httpMock: HttpTestingController;

    const apiUrl = `${environment.apiBaseUrl}/payments`;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting(),
                PaymentsService
            ]
        });

        service = TestBed.inject(PaymentsService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify(); // Ensure no unmatched requests
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getPayments', () => {
        it('should fetch all payments via GET', () => {
            const mockPayments: PaymentDto[] = [
                { id: '1', reference: 'PAY-20260405-0001', amount: 100, currency: 'USD', createdAt: '2026-04-05T10:00:00Z' },
                { id: '2', reference: 'PAY-20260405-0002', amount: 250, currency: 'EUR', createdAt: '2026-04-05T11:00:00Z' }
            ];

            service.getPayments().subscribe(payments => {
                expect(payments.length).toBe(2);
                expect(payments).toEqual(mockPayments);
            });

            const req = httpMock.expectOne(apiUrl);
            expect(req.request.method).toBe('GET');
            req.flush(mockPayments);
        });

        it('should return empty array when no payments exist', () => {
            service.getPayments().subscribe(payments => {
                expect(payments.length).toBe(0);
            });

            const req = httpMock.expectOne(apiUrl);
            req.flush([]);
        });
    });

    describe('createPayment', () => {
        it('should send POST request with payment data', () => {
            const request: CreatePaymentRequest = {
                amount: 100,
                currency: 'USD',
                clientRequestId: 'test-guid-123'
            };

            const mockResponse: PaymentDto = {
                id: 'new-id',
                reference: 'PAY-20260405-0001',
                amount: 100,
                currency: 'USD',
                createdAt: '2026-04-05T10:00:00Z'
            };

            service.createPayment(request).subscribe(payment => {
                expect(payment).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(apiUrl);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('updatePayment', () => {
        it('should send PUT request with id and updated data', () => {
            const id = 'abc-123';
            const request: UpdatePaymentRequest = {
                amount: 500,
                currency: 'GBP'
            };

            const mockResponse: PaymentDto = {
                id: id,
                reference: 'PAY-20260405-0001',
                amount: 500,
                currency: 'GBP',
                createdAt: '2026-04-05T10:00:00Z'
            };

            service.updatePayment(id, request).subscribe(payment => {
                expect(payment).toEqual(mockResponse);
                expect(payment.amount).toBe(500);
                expect(payment.currency).toBe('GBP');
            });

            const req = httpMock.expectOne(`${apiUrl}/${id}`);
            expect(req.request.method).toBe('PUT');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('deletePayment', () => {
        it('should send DELETE request with id', () => {
            const id = 'abc-123';

            service.deletePayment(id).subscribe();

            const req = httpMock.expectOne(`${apiUrl}/${id}`);
            expect(req.request.method).toBe('DELETE');
            req.flush(null, { status: 204, statusText: 'No Content' });
        });
    });

    describe('error handling', () => {
        it('should propagate HTTP error on getPayments', () => {
            service.getPayments().subscribe({
                next: () => fail('should have failed'),
                error: (error) => {
                    expect(error.status).toBe(500);
                }
            });

            const req = httpMock.expectOne(apiUrl);
            req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
        });

        it('should propagate HTTP error on createPayment', () => {
            service.createPayment({ amount: 100, currency: 'USD', clientRequestId: 'guid' }).subscribe({
                next: () => fail('should have failed'),
                error: (error) => {
                    expect(error.status).toBe(400);
                }
            });

            const req = httpMock.expectOne(apiUrl);
            req.flush('Bad request', { status: 400, statusText: 'Bad Request' });
        });
    });
});
