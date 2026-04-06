import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppComponent } from './app.component';

describe('AppComponent', () => {
    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [AppComponent],
            providers: [provideRouter([])]
        }).compileComponents();
    });

    it('should create the app', () => {
        const fixture = TestBed.createComponent(AppComponent);
        const component = fixture.componentInstance;
        expect(component).toBeTruthy();
    });

    it('should have title "Payments Portal"', () => {
        const fixture = TestBed.createComponent(AppComponent);
        const component = fixture.componentInstance;
        expect(component.title).toBe('Payments Portal');
    });
});
