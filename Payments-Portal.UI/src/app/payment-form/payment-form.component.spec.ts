import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { PaymentFormComponent } from './payment-form.component';
import { PaymentsService } from '../payments.service';
import { ClientRequestIdService } from '../services/client-request-id.service';
import { PaymentDto } from '../models/payment.model';

import { SUPPORTED_CURRENCIES } from '../models/payment.constants';

describe('PaymentFormComponent', () => {
    let component: PaymentFormComponent;
    let fixture: ComponentFixture<PaymentFormComponent>;
    let mockPaymentsService: jasmine.SpyObj<PaymentsService>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockClientRequestIdService: jasmine.SpyObj<ClientRequestIdService>;

    const mockPayments: PaymentDto[] = [
        { id: 'abc-123', reference: 'PAY-20260405-0001', amount: 100, currency: 'USD', createdAt: '2026-04-05T10:00:00Z' },
        { id: 'def-456', reference: 'PAY-20260405-0002', amount: 250, currency: 'EUR', createdAt: '2026-04-05T11:00:00Z' }
    ];

    function createComponent(routeId: string | null = null) {
        mockPaymentsService = jasmine.createSpyObj('PaymentsService', ['getPayments', 'createPayment', 'updatePayment', 'deletePayment']);
        mockPaymentsService.getPayments.and.returnValue(of(mockPayments));
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);
        mockClientRequestIdService = jasmine.createSpyObj('ClientRequestIdService', ['generate']);
        mockClientRequestIdService.generate.and.returnValue('mock-guid-12345');

        TestBed.configureTestingModule({
            imports: [PaymentFormComponent, ReactiveFormsModule],
            providers: [
                { provide: PaymentsService, useValue: mockPaymentsService },
                { provide: Router, useValue: mockRouter },
                { provide: ClientRequestIdService, useValue: mockClientRequestIdService },
                {
                    provide: ActivatedRoute,
                    useValue: {
                        snapshot: {
                            paramMap: {
                                get: (key: string) => routeId
                            }
                        }
                    }
                }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(PaymentFormComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    }

    describe('Create mode (new payment)', () => {
        beforeEach(() => {
            createComponent(null);
        });

        it('should create', () => {
            expect(component).toBeTruthy();
        });

        it('should be in create mode', () => {
            expect(component.isEditMode).toBeFalse();
            expect(component.paymentId).toBeNull();
        });

        it('should have "Add New Payment" heading', () => {
            const compiled = fixture.nativeElement as HTMLElement;
            expect(compiled.querySelector('h2')?.textContent).toContain('Add New Payment');
        });

        it('should initialize form with default values', () => {
            expect(component.paymentForm.get('amount')?.value).toBeNull();
            expect(component.paymentForm.get('currency')?.value).toBe('USD');
        });

        it('should mark form as invalid when amount is empty', () => {
            component.paymentForm.patchValue({ amount: null });
            expect(component.paymentForm.valid).toBeFalse();
        });

        it('should mark form as invalid when amount is 0', () => {
            component.paymentForm.patchValue({ amount: 0 });
            expect(component.paymentForm.valid).toBeFalse();
        });

        it('should mark form as invalid when amount is negative', () => {
            component.paymentForm.patchValue({ amount: -5 });
            expect(component.paymentForm.valid).toBeFalse();
        });

        it('should mark form as valid with proper values', () => {
            component.paymentForm.patchValue({ amount: 100, currency: 'EUR' });
            expect(component.paymentForm.valid).toBeTrue();
        });

        it('should call createPayment and navigate on submit', fakeAsync(() => {
            const createdPayment: PaymentDto = {
                id: 'new-id', reference: 'PAY-20260405-0003', amount: 150, currency: 'GBP', createdAt: '2026-04-05T12:00:00Z'
            };
            mockPaymentsService.createPayment.and.returnValue(of(createdPayment));

            component.paymentForm.patchValue({ amount: 150, currency: 'GBP' });
            component.onSubmit();
            tick();

            expect(mockClientRequestIdService.generate).toHaveBeenCalledTimes(1);
            expect(mockPaymentsService.createPayment).toHaveBeenCalledTimes(1);
            const callArgs = mockPaymentsService.createPayment.calls.mostRecent().args[0];
            expect(callArgs.amount).toBe(150);
            expect(callArgs.currency).toBe('GBP');
            expect(callArgs.clientRequestId).toBe('mock-guid-12345');
            expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
        }));

        it('should NOT submit when form is invalid', () => {
            component.paymentForm.patchValue({ amount: null });
            component.onSubmit();
            expect(mockPaymentsService.createPayment).not.toHaveBeenCalled();
        });

        it('should set submitting=false on error', fakeAsync(() => {
            mockPaymentsService.createPayment.and.returnValue(throwError(() => new Error('Server error')));
            component.paymentForm.patchValue({ amount: 100, currency: 'USD' });

            spyOn(console, 'error');
            component.onSubmit();
            tick();

            expect(component.submitting).toBeFalse();
            expect(mockRouter.navigate).not.toHaveBeenCalled();
        }));

        it('should navigate to "/" on cancel', () => {
            component.cancel();
            expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
        });
    });

    describe('Edit mode', () => {
        beforeEach(() => {
            createComponent('abc-123');
        });

        it('should be in edit mode', () => {
            expect(component.isEditMode).toBeTrue();
            expect(component.paymentId).toBe('abc-123');
        });

        it('should have "Edit Payment" heading', () => {
            const compiled = fixture.nativeElement as HTMLElement;
            expect(compiled.querySelector('h2')?.textContent).toContain('Edit Payment');
        });

        it('should load existing payment data into form', fakeAsync(() => {
            tick();

            expect(mockPaymentsService.getPayments).toHaveBeenCalled();
            expect(component.paymentForm.get('amount')?.value).toBe(100);
            expect(component.paymentForm.get('currency')?.value).toBe('USD');
        }));

        it('should call updatePayment and navigate on submit', fakeAsync(() => {
            tick(); // wait for loadPayment

            const updatedPayment: PaymentDto = {
                id: 'abc-123', reference: 'PAY-20260405-0001', amount: 999, currency: 'INR', createdAt: '2026-04-05T10:00:00Z'
            };
            mockPaymentsService.updatePayment.and.returnValue(of(updatedPayment));

            component.paymentForm.patchValue({ amount: 999, currency: 'INR' });
            component.onSubmit();
            tick();

            expect(mockPaymentsService.updatePayment).toHaveBeenCalledWith('abc-123', { amount: 999, currency: 'INR' });
            expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
        }));

        it('should handle error loading payment gracefully', fakeAsync(() => {
            // The component was already created and loadPayment was called in beforeEach.
            // Simulate an error scenario by calling loadPayment again with a failing service.
            mockPaymentsService.getPayments.and.returnValue(throwError(() => new Error('fail')));

            component.loadPayment('abc-123');
            tick();

            expect(component.loading).toBeFalse();
        }));
    });

    describe('Currency options', () => {
        beforeEach(() => {
            createComponent(null);
        });

        it('should have supported currency options from constants', () => {
            expect(component.currencies).toEqual(SUPPORTED_CURRENCIES);
        });

        it('should render currency options in select', () => {
            const compiled = fixture.nativeElement as HTMLElement;
            const options = compiled.querySelectorAll('select option');
            expect(options.length).toBe(SUPPORTED_CURRENCIES.length);
        });
    });
});
