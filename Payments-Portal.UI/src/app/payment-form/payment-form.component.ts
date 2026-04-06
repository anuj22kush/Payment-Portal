import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentsService } from '../payments.service';
import { ClientRequestIdService } from '../services/client-request-id.service';
import { SUPPORTED_CURRENCIES } from '../models/payment.constants';

@Component({
  selector: 'app-payment-form',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-form.component.html',
  styleUrl: './payment-form.component.css'
})
export class PaymentFormComponent implements OnInit {
  paymentForm: FormGroup;
  isEditMode = false;
  paymentId: string | null = null;
  loading = false;
  submitting = false;

  readonly currencies = SUPPORTED_CURRENCIES;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private paymentsService: PaymentsService,
    private clientRequestIdService: ClientRequestIdService
  ) {
    this.paymentForm = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      currency: ['USD', Validators.required]
    });
  }

  ngOnInit() {
    this.paymentId = this.route.snapshot.paramMap.get('id');
    if (this.paymentId) {
      this.isEditMode = true;
      this.loadPayment(this.paymentId);
    }
  }

  loadPayment(id: string) {
    this.loading = true;
    this.paymentsService.getPayments().subscribe({
      next: (payments) => {
        const payment = payments.find(p => p.id === id);
        if (payment) {
          this.paymentForm.patchValue({
            amount: payment.amount,
            currency: payment.currency
          });
        }
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onSubmit() {
    if (this.paymentForm.invalid) return;

    this.submitting = true;
    const formValue = this.paymentForm.value;

    if (this.isEditMode && this.paymentId) {
      this.paymentsService.updatePayment(this.paymentId, {
        amount: formValue.amount,
        currency: formValue.currency
      }).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          console.error(err);
          this.submitting = false;
        }
      });
    } else {
      this.paymentsService.createPayment({
        amount: formValue.amount,
        currency: formValue.currency,
        clientRequestId: this.clientRequestIdService.generate()
      }).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          console.error(err);
          this.submitting = false;
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/']);
  }
}
